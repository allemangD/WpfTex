using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using LatexEditor.Fonts;

namespace LatexEditor.Parser
{
    public abstract class LatexSegment
    {
        [NotNull]
        public abstract IEnumerable<GlyphInfo> Glyphs { get; }

        public static LatexSegment ToLatexSegment(IEnumerable<Token> tokens)
        {
            var tokenQ = new Queue<Token>(tokens);
            var segments = new List<LatexSegment>();
            while (tokenQ.Count > 0)
                if (PopLatexSegment(tokenQ, out var ls))
                    segments.Add(ls);

            if (segments.Count == 0)
                return new LatexNull();
            if (segments.Count == 1)
                return segments[0];
            return new LatexRun(segments);
        }

        public abstract double RelAdvWidth { get; }
        public abstract double RelAdvHeight { get; }

        private static bool PopLatexSegment(Queue<Token> tokens, out LatexSegment val)
        {
            val = null;

            if (tokens.Count == 0)
                return false;

            // todo: tabulate this

            var head = tokens.Dequeue();
            if (head.TokenName == "letter")
                val = new LatexText(CmFont.SerifItalic, head.Value);
            if (head.TokenName == "command")
            {
                if (LatexParser.GreekLetters.ContainsKey(head.Value))
                    val = new LatexText(CmFont.SerifItalic, LatexParser.GreekLetters[head.Value]);
                if (LatexParser.Spaces.ContainsKey(head.Value))
                    val = new LatexSpace(LatexParser.Spaces[head.Value]);
                if (head.Value == "^")
                    if (PopLatexSegment(tokens, out var content))
                    {
                        val = new LatexSuper(content);
                    }
                    else // to prevent possible nullref, but still incorrect.
                    {
                        val = null;
                        return false;
                    }
                if (val == null) // if no command matches
                    val = new LatexText(CmFont.SerifItalic, head.CapturedString);
            }
            if (head.TokenName == "number")
                val = new LatexText(CmFont.Serif, head.Value);
            if (head.TokenName == "escape")
                if (head.Value == "\\")
                    val = new LatexReturn(1);
            if (head.TokenName == "open")
            {
                var segments = new List<LatexSegment>();
                while (tokens.Peek().TokenName != "close")
                    if (PopLatexSegment(tokens, out var content))
                        segments.Add(content);
                tokens.Dequeue(); // toss the close
                val = new LatexRun(segments);
            }

            return val != null || PopLatexSegment(tokens, out val);
        }
    }

    public class LatexSuper : LatexSegment
    {
        public override IEnumerable<GlyphInfo> Glyphs => Content.Glyphs;

        public override double RelAdvWidth => Content.RelAdvWidth;
        public override double RelAdvHeight => Content.RelAdvHeight;

        public LatexSegment Content { get; set; }

        public LatexSuper(LatexSegment content)
        {
            Content = content;

            foreach (var gi in Content.Glyphs)
            {
                gi.RelativeOffset.Y += 1;
                gi.RelativeSize *= 0.5;
            }
        }
    }
}