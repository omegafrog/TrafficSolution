namespace TrafficForm.App
{
    [Serializable]
    public class PointOutOfRangeException : Exception
    {
        public PointOutOfRangeException()
        {
        }

        public PointOutOfRangeException(string? message) : base(message)
        {
        }

        public PointOutOfRangeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}