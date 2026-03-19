namespace TrafficForm
{
    public sealed class CctvCardClickedEventArgs : EventArgs
    {
        public CctvCardClickedEventArgs(string cctvId, string displayName, string streamUrl)
        {
            CctvId = cctvId;
            DisplayName = displayName;
            StreamUrl = streamUrl;
        }

        public string CctvId { get; }

        public string DisplayName { get; }

        public string StreamUrl { get; }
    }
}
