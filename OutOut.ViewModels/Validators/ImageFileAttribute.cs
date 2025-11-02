namespace OutOut.ViewModels.Validators
{
    public class ImageFileAttribute : FileExtensionAttribute
    {
        private static readonly string[] ImageFileExtensions = new string[] { ".png", ".jpg", ".jpeg", ".jfif", ".svg" };
        private static readonly long MaxSize = 8_388_608;
        public ImageFileAttribute() : base(ImageFileExtensions, MaxSize) { }
    }
}
