using CRReservation.COMMON.Enums.UserEnums;
using CRReservation.COMMON.States;

namespace CRReservation.COMMON.Services
{
	public class UserStateService
	{
		public User CurrentState { get; private set; } = new User();

		public event Action OnChange;

		public void SetState(bool isLoggedIn, string userName, Role role)
		{
			CurrentState.IsLoggedIn = isLoggedIn;
			CurrentState.UserName = userName;
			CurrentState.Role = role;
			NotifyStateChanged();
		}

		private void NotifyStateChanged() => OnChange?.Invoke();
	}
}
