namespace WindowsServiceDotNetCore
{
    public class RollingFileSinkOptions
    {
        public string PathFormat { get; set; }

        public int RetainedFileCountLimit { get; set; }
    }
}