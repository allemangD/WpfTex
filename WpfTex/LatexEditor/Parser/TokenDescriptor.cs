using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace LatexEditor.Parser
{
    public class TokenDescriptor
    {
        public string TokenName { get; }
        public int CaptureGroup { get; }
        public Regex Regex { get; }

        public TokenDescriptor(string tokenName, [RegexPattern, NotNull] string pattern, int captureGroup = 0)
        {
            TokenName = tokenName;
            CaptureGroup = captureGroup;
            Regex = new Regex("^" + pattern);
        }

        public bool TryBreakString(ref string text, out Token capture)
        {
            var match = Regex.Match(text);

            if (!match.Success)
            {
                capture = null;
                return false;
            }

            capture = new Token(TokenName, match.Groups[CaptureGroup].Value, match.Value);
            text = text.Substring(match.Length);
            return true;
        }
    }
}