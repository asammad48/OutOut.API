using OutOut.Constants.Enums;

namespace OutOut.ViewModels.Responses.Categories
{
    public class CategoryResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public TypeFor TypeFor { get; set; }
        public bool IsActive { get; set; }
    }
}
