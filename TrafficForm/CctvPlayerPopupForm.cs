namespace TrafficForm
{
    public sealed class CctvPlayerPopupForm : Form
    {
        public CctvPlayerPopupForm(string displayName, string cctvUrl)
        {
            string safeDisplayName = string.IsNullOrWhiteSpace(displayName) ? "CCTV" : displayName.Trim();
            _ = cctvUrl;

            Text = $"CCTV 재생 - {safeDisplayName}";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(700, 460);
            Size = new Size(1100, 760);
            BackColor = Color.FromArgb(24, 28, 36);
        }
    }
}
