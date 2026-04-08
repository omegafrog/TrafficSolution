using System.Text;
using TrafficForm.Port;

namespace TrafficForm.Adapter
{
    public class DefaultRoadNameQueryExpanderAdapter : IRoadNameQueryExpanderPort
    {
        public IReadOnlyList<string> Expand(string query)
        {
            string normalized = Normalize(query);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return Array.Empty<string>();
            }

            return new[] { normalized };
        }

        internal static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(value.Length);
            bool previousWasWhitespace = false;

            foreach (char character in value.Trim())
            {
                if (char.IsWhiteSpace(character))
                {
                    if (!previousWasWhitespace)
                    {
                        builder.Append(' ');
                    }

                    previousWasWhitespace = true;
                    continue;
                }

                builder.Append(char.ToLowerInvariant(character));
                previousWasWhitespace = false;
            }

            return builder.ToString();
        }
    }
}
