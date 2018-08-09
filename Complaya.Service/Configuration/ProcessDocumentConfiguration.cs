namespace Complaya.Service
{
    public class ProcessDocumentConfiguration
    {
        public bool RunOnceImmediately { get; set; }
        public int RunEveryXSec { get; set; }
        public RunAt RunAt { get; set; }
    }
}