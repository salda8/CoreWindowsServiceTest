namespace Complaya
{
    public class FolderWatcherConfiguration
    {
        public string Path { get; set; }
        public string Filter { get; set; } = "*.pdf";

        public bool ShouldProcessAlreadyExistingFiles { get; set; }

    }


}
