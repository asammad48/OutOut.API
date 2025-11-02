namespace OutOut.Models
{
    public class SMTPConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Mail { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
    }

    public class DefaultUserLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description { get; set; }
    }

    public class Directories
    {
        public string ProfileImages { get; set; }
        public string VenuesMenus { get; set; }
        public string CategoryIcons { get; set; }
        public string TypeIcons { get; set; }
        public string VenuesGallery { get; set; }
        public string VenuesLogos { get; set; }
        public string EventsImages { get; set; }
        public string OffersImages { get; set; }
        public string NotificationsIcons { get; set; }
    }

    public class TelrConfigurations
    {
        public string StoreId { get; set; }
        public string AuthKey { get; set; }
    }

    public class TokenDuration
    {
        public int Days { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
    }

    public class FCMConfiguration
    {
        public string ClientConfigurationFileName { get; set; }
    }

    public class AppSecrets
    {
        public string JWTSecretKey { get; set; }
        public string FacebookAppId { get; set; }
        public string FacebookAppSecret { get; set; }
        public string GoogleApiKey { get; set; }
        public List<string> GoogleClientIds { get; set; }
        public string AppleClientId { get; set; }
    }

    public class Connections
    {
        public string NonSqlConnectionString { get; set; }
        public string NonSqlDatabaseName { get; set; }
        public string NonSqlHangfireConnectionString { get; set; }
        public Dictionary<string, string> NonSqlCollectionsNames { get; set; }
    }

    public class OTPConfigurations
    {
        public int Length { get; set; }
        public int ValidForMinutes { get; set; }
    }

    public class SignalRGroup
    {
        public string SuperAdmins { get; set; }
        public string VenueAdmins { get; set; }
        public string EventAdmins { get; set; }
    }

    public class AppSettings
    {
        public Connections Connections { get; set; }
        public AppSecrets AppSecrets { get; set; }
        public SMTPConfiguration SMTPConfiguration { get; set; }
        public DefaultUserLocation DefaultUserLocation { get; set; }
        public List<string> DevelopersEmails { get; set; }
        public FCMConfiguration FCMConfigurations { get; set; }
        public TokenDuration JWTRefreshTokenDuration { get; set; }
        public TokenDuration JWTTokenDuration { get; set; }
        public Directories Directories { get; set; }
        public OTPConfigurations OTPConfigurations { get; set; }
        public string BackendOrigin { get; set; }
        public string FrontendOrigin { get; set; }
        public string SuperAdminEmail { get; set; }
        public List<string> AllowedCountries { get; set; }
        public double UserRadius { get; set; }
        public int RemindersMinutesDelay { get; set; }
        public int PaymentRecoveryMinutesDelay { get; set; }
        public string CountriesFileName { get; set; }
        public TelrConfigurations TelrConfigurations { get; set; }
        public int TimeZoneOffset { get; set; }
        public List<string> AllowedMobileVersions { get; set; }
        public string GeoLocationAPIKey { get; set; }
        public SignalRGroup SignalRGroup { get; set; }
    }
}
