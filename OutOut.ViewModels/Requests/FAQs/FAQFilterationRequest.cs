namespace OutOut.ViewModels.Requests.FAQs
{
    public class FAQFilterationRequest
    {
        public string SearchQuery { get; set; }
        public SortFAQ SortBy { get; set; }
    }

    public enum SortFAQ
    {
        QuestionNumber, Alphabetical
    }
}
