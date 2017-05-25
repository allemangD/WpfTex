using System.Collections.Generic;
using JetBrains.Annotations;
using LatexEditor.Fonts;

namespace LatexEditor.Parser.Segments
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
                if (Parser.GreekLetters.ContainsKey(head.Value))
                    val = new LatexText(CmFont.SerifItalic, Parser.GreekLetters[head.Value]);
                if (Parser.Spaces.ContainsKey(head.Value))
                    val = new LatexSpace(Parser.Spaces[head.Value]);
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
                // todo: fix incorrect glyph placement within {}
                // Should create a LatexGlyph: LatexSegment, and change LatexSegment.Glyphs 
                // to be of LatexGlyphs.Then, ON ITERATION, not creation, compute the altered positions of
                // the LatexGlyphs and create the GlyphRuns from that.
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

}