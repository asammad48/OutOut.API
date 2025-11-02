namespace OutOut.ViewModels.Validators
{
    public class DocumentFileAttribute : FileExtensionAttribute
    {
        private static readonly long MaxSize = 4194304;
        public static readonly string[] DocumentFileExtensions = { ".pdf" };
        public DocumentFileAttribute() : base(DocumentFileExtensions, MaxSize) { }
    }
}
