using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JetBrains.Annotations;
using LatexEditor.Fonts;

namespace LatexEditor.Parser.Segments
{
    public abstract class LatexSegment
    {
        public List<LatexSegment> Contents { get; } = new List<LatexSegment>();

        public abstract double Width { get; }
        public abstract double Height { get; }

        public double Size { get; protected set; } = 1;
        public Point Offset { get; protected set; } = new Point(0, 0);

        public abstract IEnumerable<LatexGlyph> Glyphs { get; }

        public static LatexSegment ToLatexSegment(IEnumerable<Token> tokens)
        {
            var tokenQ = new Queue<Token>(tokens);
            var segments = new List<LatexSegment>();
            while (tokenQ.Count > 0)
                if (PopLatexSegment(tokenQ, out var ls))
                    segments.Add(ls);

            if (segments.Count == 0)
                return null;
            if (segments.Count == 1)
                return segments[0];
            return new LatexNull(segments);
        }

        private static bool PopLatexSegment(Queue<Token> tokens, out LatexSegment val)
        {
            val = null;

            if (tokens.Count == 0)
                return false;

            // todo: tabulate this

            var head = tokens.Dequeue();
            switch (head.TokenName)
            {
                case "letter":
                    val = new LatexGlyph(CmFont.SerifItalic, head.Value[0]);
                    break;

                case "number":
                    val = new LatexGlyph(CmFont.Serif, head.Value[0]);
                    break;

                case "open": // todo: implement proper lookahead
                    // currently crashes if no closing brace present
                    val = new LatexNull();
                    while (tokens.Peek().TokenName != "close")
                        if (PopLatexSegment(tokens, out var content))
                            val.Contents.Add(content);
                    tokens.Dequeue(); // pop close token
                    break;

                case "command":
                    if (LatexParser.GreekLetters.ContainsKey(head.Value))
                        val = new LatexGlyph(CmFont.Serif, LatexParser.GreekLetters[head.Value]);
                    if (LatexParser.Spaces.ContainsKey(head.Value))
                        val = new LatexSpace(LatexParser.Spaces[head.Value]);
                    switch (head.Value)
                    {
                        case "^":
                        case "sup":
                            if (PopLatexSegment(tokens, out var content))
                            {
                                val = content;
                                val.Size *= 0.7;
                                val.Offset = new Point(val.Offset.X, val.Offset.Y + 0.45);
                            }
                            break;
                        case "_":
                        case "sub":
                            if (PopLatexSegment(tokens, out content))
                            {
                                val = content;
                                val.Size *= 0.7;
                                val.Offset = new Point(val.Offset.X, val.Offset.Y - 0.45);
                            }
                            break;
                    }
                    break;

                case "escape":
                    if (head.Value == "\\")
                        val = new LatexReturn();
                    else
                        val = new LatexGlyph(CmFont.Serif, head.Value[0]);
                    break;
            }

            return val != null || PopLatexSegment(tokens, out val);
        }

        public override string ToString()
        {
            return $"{GetType().Name}: [{string.Join(", ", Contents.Select(c => "{" + c + "}"))}]";
        }


        public virtual IEnumerable<GlyphDescriptor> GlyphDescriptors
        {
            get
            {
                var o_x = 0d;
                var o_y = 0d;
                foreach (var seg in Contents)
                {
                    if (seg is LatexReturn)
                    {
                        o_x = 0;
                        o_y -= 1;
                        continue;
                    }
                    foreach (var gd in seg.GlyphDescriptors)
                    {
                        var cp = gd;
                        cp.Size *= Size;
                        cp.Offset.X += o_x + Offset.X;
                        cp.Offset.Y += o_y + Offset.Y;
                        yield return cp;
                    }

                    o_x += seg.Width * Size;
                }
            }
        }
    }

    public class LatexGlyph : LatexSegment
    {
        private GlyphDescriptor _glyphDescriptor;
        public override double Width => _glyphDescriptor.AdvanceWidth * Size;
        public override double Height => _glyphDescriptor.AdvanceHeight * Size;

        public override IEnumerable<LatexGlyph> Glyphs
        {
            get { yield return this; }
        }

        public int CharId { get; }

        public char Char => (char) CharId;

        public LatexGlyph(CmFont font, int charId)
        {
            CharId = charId;
            _glyphDescriptor = new GlyphDescriptor(font.GlyphTypeface(), CharId);
        }

        public override string ToString()
        {
            return $"{base.ToString()}, '{Char}' ({CharId})";
        }

        public override IEnumerable<GlyphDescriptor> GlyphDescriptors
        {
            get
            {
                var cp = _glyphDescriptor;
                cp.Size *= Size;
                cp.Offset.X += Offset.X;
                cp.Offset.Y += Offset.Y;
                yield return cp;
            }
        }

        // todo: essentially duplicate glyphinfo functionality here
        // need to make sure that parent has control over offset and new relative size.
        // Width and Height should account for this.
        // easiest solution would be an "offset" method that returns a modified clone.
    }

    public class LatexReturn : LatexSegment
    {
        public override double Width => 0;
        public override double Height => 0;
        public override IEnumerable<LatexGlyph> Glyphs => Enumerable.Empty<LatexGlyph>();
    }

    // todo: should be able to implement LatexReturn through latexspace.
    public class LatexSpace : LatexSegment
    {
        public override double Width { get; }
        public override double Height { get; }
        public override IEnumerable<LatexGlyph> Glyphs => Enumerable.Empty<LatexGlyph>();

        public LatexSpace(double width, double height = 0)
        {
            Width = width;
            Height = height;
        }
    }

    public class LatexNull : LatexSegment
    {
        public override double Width => Contents.Sum(ls => ls.Width);
        public override double Height => Contents.Max(ls => ls.Height);

        public override IEnumerable<LatexGlyph> Glyphs
        {
            get
            {
                foreach (var ls in Contents)
                foreach (var lg in ls.Glyphs)
                    yield return lg;
            }
        }

        public LatexNull()
        {
        }

        public LatexNull(IEnumerable<LatexSegment> contents)
        {
            Contents.AddRange(contents);
        }
    }
}