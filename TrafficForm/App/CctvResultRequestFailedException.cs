namespace TrafficForm.App
{
    [Serializable]
    public class CctvResultRequestFailedException : Exception
    {
        public CctvResultRequestFailedException()
        {
        }

        public CctvResultRequestFailedException(string? message) : base(message)
        {
        }

        public CctvResultRequestFailedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
