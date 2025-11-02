namespace OutOut.ViewModels.Validators
{
    public class ImageFilesAttribute : FilesExtensionAttribute
    {
        private static readonly string[] ImageFileExtensions = new string[] { ".png", ".jpg", ".jpeg", ".jfif", ".svg" };
        private static readonly long MaxSize = 8_388_608;
        public ImageFilesAttribute() : base(ImageFileExtensions, MaxSize) { }
    }
}
