namespace OutOut.ViewModels.Requests.ManageAdminDashboard
{
    public class FilterationRequest
    {
        public string SearchQuery { get; set; }
        public Sort SortBy { get; set; }
    }

    public enum Sort
    {
        Newest, Alphabetical, Date
    }
}
