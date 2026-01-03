using Microsoft.JSInterop;

namespace CRReservation.COMMON.Services;

public interface ITokenService
{
    Task<string?> GetTokenAsync();
    Task SaveTokenAsync(string token);
    Task RemoveTokenAsync();
    Task InitializeAsync();
}

public class TokenService : ITokenService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;
    private const string TokenKey = "auth_token";
    private const string ModulePath = "./js/localStorage.js";

    public TokenService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);
        }
        catch
        {
            // localStorage bêdzie dostêpny bez modu³u, mo¿emy u¿yæ bezpoœrednio
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            if (_module != null)
            {
                return await _module.InvokeAsync<string?>("getItem", TokenKey);
            }
            else
            {
                return await _jsRuntime.InvokeAsync<string?>("eval", $"localStorage.getItem('{TokenKey}')");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting token: {ex.Message}");
            return null;
        }
    }

    public async Task SaveTokenAsync(string token)
    {
        try
        {
            if (_module != null)
            {
                await _module.InvokeVoidAsync("setItem", TokenKey, token);
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("eval", $"localStorage.setItem('{TokenKey}', '{token}')");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving token: {ex.Message}");
        }
    }

    public async Task RemoveTokenAsync()
    {
        try
        {
            if (_module != null)
            {
                await _module.InvokeVoidAsync("removeItem", TokenKey);
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("eval", $"localStorage.removeItem('{TokenKey}')");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error removing token: {ex.Message}");
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch
            {
                // Ignore disposal errors
            }
        }
    }
}

