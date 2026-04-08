namespace TrafficForm.Port
{
    public interface IRoadNameQueryExpanderPort
    {
        IReadOnlyList<string> Expand(string query);
    }
}
