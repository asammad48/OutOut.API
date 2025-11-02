using Microsoft.AspNetCore.Identity;
using OutOut.Constants;
using OutOut.Models.Identity;
using OutOut.Persistence.Interfaces;

namespace OutOut.Persistence.Providers
{
    public class UserDetailsProvider : IUserDetailsProvider
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVenueRepository _venueRepository;

        public UserDetailsProvider(UserManager<ApplicationUser> userManager, IVenueRepository venueRepository)
        {
            _userManager = userManager;
            _venueRepository = venueRepository;
        }

        public string UserId { get; private set; }
        public ApplicationUser User { get; private set; }
        public List<string> UserRoles { get; private set; }
        public bool IsAdmin { get; private set; }
        public bool IsSuperAdmin { get; private set; }
        public string AccessToken { get; private set; }

        public void Initialize(ApplicationUser user, List<string> userRoles, string accessToken)
        {
            UserId = user.Id;
            User = user;
            UserRoles = userRoles;
            AccessToken = accessToken;
            IsAdmin = UserRoles.Intersect(Roles.ALL_ROLES).Any();
            IsSuperAdmin = UserRoles.Contains(Roles.SuperAdmin);
        }

        public void InitializeUnAuthenticated()
        {
            User = new ApplicationUser();
            User.Roles = new List<string>();
        }

        public async Task ReInitialize()
        {
            User = await _userManager.FindByIdAsync(UserId);
            UserRoles = (await _userManager.GetRolesAsync(User)).ToList();
            IsAdmin = UserRoles.Intersect(Roles.ALL_ROLES).Any();
            IsSuperAdmin = UserRoles.Contains(Roles.SuperAdmin);
        }

        public bool IsInRole(string role)
        {
            return UserRoles?.Contains(role) ?? false;
        }

        public bool HasAccessToVenue(string venueId)
        {
            return User?.AccessibleVenues?.Contains(venueId) ?? false;
        }

        public bool HasAccessToEvent(string eventId)
        {
            return GetAccessibleEvents()?.Contains(eventId) ?? false;
        }

        public List<string> GetAccessibleEvents()
        {
            var accessibleEvents = User.AccessibleEvents;
            if (UserRoles.Contains(Roles.VenueAdmin))
            {
                foreach (var venueId in User.AccessibleVenues)
                {
                    var venue = _venueRepository.GetVenueById(venueId);
                    if (venue != null) accessibleEvents.AddRange(venue.Events);
                }
            }
            return accessibleEvents.Distinct().ToList();
        }

        public List<string> GetSuperAdmins() =>
            _userManager.GetUsersInRoleAsync(Roles.SuperAdmin).Result.Select(a => a.Id).ToList();

        public List<string> GetVenueAdmins() =>
           _userManager.GetUsersInRoleAsync(Roles.VenueAdmin).Result.Select(a => a.Id).ToList();

        public List<string> GetEventAdmins() =>
           _userManager.GetUsersInRoleAsync(Roles.EventAdmin).Result.Select(a => a.Id).ToList();

        public List<string> GetVenueAdminsWithAccessibleVenue(string accessibleVenueId) =>
           _userManager.GetUsersInRoleAsync(Roles.VenueAdmin).Result.Where(a => a.AccessibleVenues.Contains(accessibleVenueId))
                                                                    .Select(a => a.Id)
                                                                    .ToList();

        public List<string> GetVenueAdminsWithAccessibleEvent(string accessibleEventId)
        {
            var venueAdminsWithAccessibleEvent = new List<string>();

            var venueAdmins = _userManager.GetUsersInRoleAsync(Roles.VenueAdmin).Result;
            foreach (var venueAdmin in venueAdmins)
            {
                var allAccessibleVenues = venueAdmins.SelectMany(a => a.AccessibleVenues).ToList();
                var venues = _venueRepository.GetVenuesByIds(allAccessibleVenues);

                var associatedAccessibleEvents = new List<string>();
                venues.ForEach(venue => { if (venue != null) associatedAccessibleEvents.AddRange(venue.Events); });

                if (associatedAccessibleEvents.Contains(accessibleEventId) || venueAdmin.AccessibleEvents.Contains(accessibleEventId))
                    venueAdminsWithAccessibleEvent.Add(venueAdmin.Id);
            }

            return venueAdminsWithAccessibleEvent;
        }

        public List<string> GetEventAdminsWithAccessibleEvent(string accessibleEventId) =>
         _userManager.GetUsersInRoleAsync(Roles.EventAdmin).Result.Where(a => a.AccessibleEvents.Contains(accessibleEventId))
                                                                  .Select(a => a.Id)
                                                                  .ToList();

        public List<ApplicationUser> GetVenueAdminsByVenueId(string accessibleVenueId)
        {
            var venueAdminsIds = GetVenueAdminsWithAccessibleVenue(accessibleVenueId);
            return _userManager.Users.Where(u => venueAdminsIds.Contains(u.Id)).ToList();
        }
    }
}
