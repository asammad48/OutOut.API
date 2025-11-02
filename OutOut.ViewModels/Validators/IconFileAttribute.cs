namespace OutOut.ViewModels.Validators
{
    public class IconFileAttribute : FileExtensionAttribute
    {
        private static readonly string[] ImageFileExtensions = new string[] { ".png", ".jpg", ".jpeg", ".jfif", ".svg" };
        private static readonly long MaxSize = 1_048_576; //bytes in binary
        public IconFileAttribute() : base(ImageFileExtensions, MaxSize) { }
    }
}
