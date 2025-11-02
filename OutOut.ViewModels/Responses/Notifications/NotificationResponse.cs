using OutOut.Constants.Enums;
using System;

namespace OutOut.ViewModels.Responses.Notifications
{
    public class NotificationResponse
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Image { get; set; }
        public NotificationAction Action { get; set; }
        public DateTime SentDate { get; set; }
        public string AffectedId { get; set; }
        public string UpdatedEntityId { get; set; }
        public bool IsRead { get; set; }
    }
}
