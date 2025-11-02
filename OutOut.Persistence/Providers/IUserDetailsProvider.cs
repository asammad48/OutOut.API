using OutOut.Models.Identity;

namespace OutOut.Persistence.Providers
{
    public interface IUserDetailsProvider
    {
        public string UserId { get; }
        public ApplicationUser User { get; }
        public List<string> UserRoles { get; }
        public bool IsAdmin { get; }
        public bool IsSuperAdmin { get; }
        public string AccessToken { get; }
        public void InitializeUnAuthenticated();
        public void Initialize(ApplicationUser user, List<string> userRoles, string accessToken);
        public Task ReInitialize();
        public bool IsInRole(string role);
        bool HasAccessToVenue(string venueId);
        bool HasAccessToEvent(string eventId);
        List<string> GetAccessibleEvents();
        List<string> GetSuperAdmins();
        List<string> GetVenueAdmins();
        List<string> GetEventAdmins();
        List<string> GetVenueAdminsWithAccessibleVenue(string accessibleVenueId);
        List<string> GetVenueAdminsWithAccessibleEvent(string accessibleEventId);
        List<string> GetEventAdminsWithAccessibleEvent(string accessibleEventId);
        List<ApplicationUser> GetVenueAdminsByVenueId(string accessibleVenueId);

    }
}
