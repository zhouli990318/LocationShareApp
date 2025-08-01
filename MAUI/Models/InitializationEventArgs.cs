namespace LocationShareApp.Models
{
    public class InitializationEventArgs : EventArgs
    {
        public string Step { get; set; } = string.Empty;
        public int Progress { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string? Message { get; set; }
    }
}