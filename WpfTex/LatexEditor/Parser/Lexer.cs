using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace LatexEditor.Parser
{

    public class Lexer : IEnumerable<TokenDescriptor>
    {
        public List<TokenDescriptor> TokenDescriptors { get; }

        public Lexer() : this(new List<TokenDescriptor>())
        {
        }

        public Lexer(List<TokenDescriptor> tokens)
        {
            TokenDescriptors = tokens;
        }

        public IEnumerable<Token> Tokenize(string text)
        {
            while (!string.IsNullOrEmpty(text))
            {
                Token cap = null;
                foreach (var ltd in TokenDescriptors)
                    if (ltd.TryBreakString(ref text, out cap))
                        break;

                if (cap == null)
                    text = text.Substring(1);
                else
                    yield return cap;
            }
        }

        public void Add(TokenDescriptor td) => TokenDescriptors.Add(td);

        public IEnumerator<TokenDescriptor> GetEnumerator()
        {
            return TokenDescriptors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) TokenDescriptors).GetEnumerator();
        }
    }
}