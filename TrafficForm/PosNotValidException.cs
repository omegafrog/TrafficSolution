namespace TrafficForm
{
    [Serializable]
    internal class PosNotValidException : Exception
    {
        public PosNotValidException()
        {
        }

        public PosNotValidException(string? message) : base(message)
        {
        }

        public PosNotValidException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}