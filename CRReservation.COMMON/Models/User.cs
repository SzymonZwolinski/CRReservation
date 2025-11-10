using CRReservation.COMMON.Enums.UserEnums;

namespace CRReservation.COMMON.States
{
    public class User
    {
        public bool IsLoggedIn { get; set; }
        public string UserName { get; set; }
        public Role Role { get; set; }
    }
}
