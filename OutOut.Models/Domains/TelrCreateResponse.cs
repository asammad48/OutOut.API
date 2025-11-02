namespace OutOut.Models.Domains
{
    public class TelrCreateResponse
    {
        public TelrOrderCreateResponse Order { get; set; }
    }

    public class TelrOrderCreateResponse
    {
        public string Ref { set; get; }
        public string Url { set; get; }
    }
}
