using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization.Attributes;
using OutOut.Constants.Enums;
using OutOut.Models.Models;

namespace OutOut.Models.Identity
{
    [BsonIgnoreExtraElements]
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser() : base()
        {
            ExternalProvider = ExternalProvider.None;

            Roles = new List<string>();
            Claims = new List<IdentityUserClaim<string>>();
            Logins = new List<IdentityUserLogin<string>>();
            Tokens = new List<IdentityUserToken<string>>();
            RefreshTokens = new List<IdentityRefreshToken>();
            FirebaseMessagingTokens = new List<string>();
            FavoriteVenues = new List<string>();
            FavoriteEvents = new List<string>();
            SharedTickets = new List<SharedTicket>();
            AccessibleVenues = new List<string>();
            AccessibleEvents = new List<string>();
            LastUsage = new LastUsage();
            CreationDate = DateTime.UtcNow;

            NotificationsAllowed = true;
            RemindersAllowed = true;
        }
        public LastUsage LastUsage { get; set; }
        public List<SharedTicket> SharedTickets { get; set; }
        public ExternalProvider ExternalProvider { get; set; }
        public string FullName { get; set; }
        public Gender Gender { get; set; }
        public string ProfileImage { get; set; }
        public UserLocation Location { get; set; }
        public List<string> FirebaseMessagingTokens { get; set; }
        public bool NotificationsAllowed { get; set; }
        public bool RemindersAllowed { get; set; }
        public List<string> FavoriteVenues { get; set; }
        public List<string> FavoriteEvents { get; set; }
        public string CompanyName { get; set; }
        public List<string> AccessibleVenues { get; set; }
        public List<string> AccessibleEvents { get; set; }
        public DateTime CreationDate { get; set; }

        #region Authentication
        public UserOTP VerificationOTP { get; set; } = new UserOTP();
        public UserOTP ResetPasswordOTP { get; set; } = new UserOTP();
        public string AuthenticatorKey { get; set; }
        public List<string> Roles { get; set; }
        public List<IdentityUserClaim<string>> Claims { get; set; }
        public List<IdentityUserLogin<string>> Logins { get; set; }
        public List<IdentityUserToken<string>> Tokens { get; set; }
        public List<IdentityRefreshToken> RefreshTokens { get; set; }
        public List<TwoFactorRecoveryCode> RecoveryCodes { get; set; }

        #endregion
    }
    public class TwoFactorRecoveryCode
    {
        public string Code { get; set; }
        public bool Redeemed { get; set; }
    }

    public class LastUsage
    {
        public LastUsage()
        {
            LastUsageDate = DateTime.UtcNow.Date;
            LastNotificationSentDate = DateTime.MinValue;
        }
        public DateTime LastUsageDate { get; set; }
        public DateTime LastNotificationSentDate { get; set; }
    }

    public class SharedTicket
    {
        public SharedTicket(string sharedBy, string bookingId, string ticketId, string ticketSecret)
        {
            SharedBy = sharedBy;
            BookingId = bookingId;
            TicketId = ticketId;
            TicketSecret = ticketSecret;
            ReceivedDate = DateTime.UtcNow;
        }
        public string SharedBy { get; set; } //UserId
        public string TicketId { get; set; }
        public string TicketSecret { get; set; }
        public string BookingId { get; set; }
        public DateTime ReceivedDate { get; set; }
        public List<ReminderType> Reminders { get; set; }
    }
}