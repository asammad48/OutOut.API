namespace OutOut.ViewModels.Responses.Excel
{
    public class FileResponse
    {
        public FileResponse(byte[] file, string fileName)
        {
            File = file;
            FileName = fileName;
        }
        public FileResponse() { }

        public byte[] File { get; set; }
        public string FileName { get; set; }
    }
}
