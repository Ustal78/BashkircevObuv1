namespace BashkircevObuv
{
    public static class CurrentUserClass
    {
        public static Users user { get; set; }

        public static bool IsAdmin
        {
            get
            {
                return user != null && user.RoleID == 1;
            }
        }

        public static bool IsManager
        {
            get
            {
                return user != null && user.RoleID == 2;
            }
        }

        public static bool IsUser
        {
            get
            {
                return user != null && user.RoleID == 3;
            }
        }

        public static void Logout()
        {
            user = null;
        }
    }
}