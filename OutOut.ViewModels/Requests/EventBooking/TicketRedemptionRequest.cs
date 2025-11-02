using System.ComponentModel.DataAnnotations;

namespace OutOut.ViewModels.Requests.EventBookings
{
    public class TicketRedemptionRequest
    {
        public string TicketId { get; set; }
        public string EventCode { get; set; }
        public string TicketSecret { get; set; }
    }
    
    public class QrTicketRedemptionRequest
    {
        [Required]
        public string TicketId { get; set; }
        [Required]
        public string TicketSecret { get; set; }
        [Required]
        public string UserId { get; set; }
    }
    
    public class TicketStatusRequest
    {
        [Required]
        public string TicketId { get; set; }
        [Required]
        public string TicketSecret { get; set; }
        [Required]
        public string UserId { get; set; }
    }
    
    public class TicketRejectionRequest
    {
        [Required]
        public string TicketId { get; set; }
        [Required]
        public string TicketSecret { get; set; }
        [Required]
        public string RejectionReason { get; set; }
        [Required]
        public string UserId { get; set; }
    }
}
