namespace OutOut.Models.Domains
{
    public class TelrCheckResponse
    {
        public TelrOrderCheckResponse Order { get; set; }
    }
    public class TelrOrderCheckResponse
    {
        public string Ref { set; get; }
        public string CartId { set; get; }
        public string Currency { set; get; }
        public string Amount { set; get; }
        public string Description { set; get; }
        public TelrOrderCheckStatus Status { set; get; }
        public TelrTransactionCheckStatus Transaction { set; get; }
        public string Paymethod { get; set; } //card
    }
    public class TelrOrderCheckStatus
    {
        public string Text { get; set; }
    }
    public class TelrTransactionCheckStatus
    {
        public string Ref { get; set; }
        public string Date { get; set; }
        public string Status { get; set; } //A, D, C
        public string Message { get; set; } //Authorized, Declined, Cancelled
    }
}
