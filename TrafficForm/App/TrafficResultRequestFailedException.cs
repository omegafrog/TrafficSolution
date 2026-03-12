namespace TrafficForm.App
{
    [Serializable]
    public class TrafficResultRequestFailedException : Exception
    {
        public TrafficResultRequestFailedException()
        {
        }

        public TrafficResultRequestFailedException(string? message) : base(message)
        {
        }

        public TrafficResultRequestFailedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}