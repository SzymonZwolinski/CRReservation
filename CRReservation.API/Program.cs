using CRReservation.API.Data;
using CRReservation.API.Models;
using CRReservation.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext - SQLite for development, can be switched to SQL Server in production
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        // Default to SQLite for development
        options.UseSqlite("Data Source=CRReservation.db");
    }
    else
    {
        // Use SQL Server in production
        options.UseSqlServer(connectionString);
    }
});

// Add Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "CRReservation.API",
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "CRReservation.Client",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"] ?? "DefaultSecretKeyForDevelopmentOnly123!"))
    };
});

//Add Authorization with policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy =>
        policy.RequireRole("admin"));
options.AddPolicy("RequireLecturerRole", policy =>
    policy.RequireRole("prowadzacy"));
options.AddPolicy("RequireStudentRole", policy =>
    policy.RequireRole("student"));

// Custom policies for reservations
options.AddPolicy("CanCreateReservation", policy =>
    policy.RequireRole("student", "prowadzacy", "admin"));
options.AddPolicy("CanApproveReservation", policy =>
    policy.RequireRole("admin"));
options.AddPolicy("CanManageRooms", policy =>
    policy.RequireRole("admin"));
});

// Register services
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure CORS
if (app.Environment.IsDevelopment())
{
    // Development: Allow any origin without credentials
    app.UseCors(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
}
else
{
    // Production: Use specific allowed origins
    app.UseCors(policy =>
    {
        policy.WithOrigins(
            builder.Configuration["Cors:AllowedOrigins"]?.Split(";")
        )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    await SeedData.InitializeAsync(dbContext);
}

app.Run();