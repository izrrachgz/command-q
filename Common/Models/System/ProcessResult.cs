namespace Common.Models.System
{
    public class ProcessResult
    {
        public string StdOutput { get; set; } = string.Empty;
        public string StdError { get; set; } = string.Empty;
        public int ExitCode { get; set; }
    }
}
