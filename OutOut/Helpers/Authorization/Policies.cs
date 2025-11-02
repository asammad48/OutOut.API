namespace OutOut.Helpers.Authorization
{
    public class Policies
    {
        public const string Only_SuperAdmin = "Only_SuperAdmin";
        public const string Only_VenueAdmins = "Only_VenueAdmin";
        public const string Only_EventAdmins = "Only_EventAdmin";

        public const string SuperAdmin_Or_VenueAdmins = "SuperAdmin_Or_VenueAdmin";
        public const string SuperAdmin_Or_EventAdmins = "SuperAdmin_Or_EventAdmin";

        public const string Admins = "Admins";
    }
}
