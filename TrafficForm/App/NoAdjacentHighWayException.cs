namespace TrafficForm.App
{
    [Serializable]
    public class NoAdjacentHighWayException : Exception
    {
        public NoAdjacentHighWayException()
        {
        }

        public NoAdjacentHighWayException(string? message) : base(message)
        {
        }

        public NoAdjacentHighWayException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}