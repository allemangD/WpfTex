namespace LatexEditor.Parser
{
    public class Token
    {
        public string TokenName { get; }
        public string Value { get; }
        public string CapturedString { get; }

        public Token(string tokenName, string value, string capturedString)
        {
            TokenName = tokenName;
            Value = value;
            CapturedString = capturedString;
        }

        public override string ToString()
        {
            return $"\"{Value}\" ({TokenName})";
        }
    }
}