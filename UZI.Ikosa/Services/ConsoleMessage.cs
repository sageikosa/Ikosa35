namespace Uzi.Ikosa.Services
{
    /// <summary>Used to instrument services and provide feedback</summary>
    public class ConsoleMessage
    {
        public object Source { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}
