using OutOut.Constants.Enums;
using OutOut.ViewModels.Validators;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.Reminders
{
    public class BookingReminderRequest
    {
        [Required]
        [MongoId]
        public string BookingId { get; set; }
        [Required]
        public List<ReminderType> ReminderTypes { get; set; }
    }
}
