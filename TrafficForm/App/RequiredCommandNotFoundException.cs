namespace TrafficForm.App
{
    [Serializable]
    internal class RequiredCommandNotFoundException : Exception
    {
        public RequiredCommandNotFoundException()
        {
        }

        public RequiredCommandNotFoundException(string? message) : base(message)
        {
        }

        public RequiredCommandNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}