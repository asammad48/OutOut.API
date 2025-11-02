using OutOut.Constants.Enums;
using System;

namespace OutOut.ViewModels.Responses.Notifications
{
    public class NotificationAdminResponse
    {
        public string Id { get; set; }
        public string Body { get; set; }
        public NotificationAction Action { get; set; }
        public string AffectedId { get; set; }
        public DateTime SentDate { get; set; }
        public bool IsRead { get; set; }
    }
}
