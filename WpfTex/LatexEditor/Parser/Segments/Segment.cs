using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Documents.DocumentStructures;
using JetBrains.Annotations;
using LatexEditor.Fonts;

namespace LatexEditor.Parser.Segments
{
    public class SegmentCollection : IEnumerable<Segment>
    {
        private List<Token> _tokens;

        public SegmentCollection(IEnumerable<Token> tokens)
        {
            _tokens = tokens.ToList();
        }

        public IEnumerator<Segment> GetEnumerator()
        {
            var tokenIndex = 0;
            while (tokenIndex < _tokens.Count)
            {
                var seg = SegmentAt(tokenIndex, out int count);

                if (seg == null) yield break;

                yield return seg;
                tokenIndex += count;
            }
        }

        private Segment SegmentAt(int index, out int numTokens)
        {
            numTokens = 1;

            if (index > _tokens.Count)
                return null;

            // todo: tabulate this

            var tok = _tokens[index];
            if (tok.TokenName == "letter")
                return new GlyphSegment(CmFont.SerifItalic, tok.Value[0]);
            if (tok.TokenName == "number")
                return new GlyphSegment(CmFont.Serif, tok.Value[0]);
            if (tok.TokenName == "open")
            {
                var seg = new NullSegment();
                var i = index + 1;
                var total = 1;
                while (true)
                {
                    if (i >= _tokens.Count)
                        return null;

                    if (_tokens[i].TokenName == "close")
                    {
                        numTokens = total + 2;
                        return seg;
                    }

                    var content = SegmentAt(i, out var n);
                    if (content == null)
                        return null;
                    seg.Contents.Add(content);
                    total += n;
                    i += n;
                }
            }
            if (tok.TokenName == "command")
            {
                if (LatexParser.GreekLetters.ContainsKey(tok.Value))
                    return new GlyphSegment(CmFont.Serif, LatexParser.GreekLetters[tok.Value]);
                if (LatexParser.Spaces.ContainsKey(tok.Value))
                    return new Space(LatexParser.Spaces[tok.Value]);
                if (tok.Value == "^")
                {
                    var sup = SegmentAt(index + 1, out var n);
                    if (sup == null)
                        return null;
                    numTokens = n + 1;

                    Segment sub = new NullSegment();

                    if (index + numTokens < _tokens.Count)
                    {
                        var tok2 = _tokens[index + numTokens];
                        Debug.WriteLine(tok2);
                        if (tok2.TokenName == "command" && tok2.Value == "_")
                        {
                            var s = SegmentAt(index + numTokens + 1, out n);
                            if (s != null)
                            {
                                sub = s;
                                numTokens += n + 1;
                            }
                        }
                    }

                    return new SupSub(sup, sub);
                }
                if (tok.Value == "_")
                {
                    var sub = SegmentAt(index + 1, out var n);
                    if (sub == null)
                        return null;
                    numTokens = n + 1;

                    Segment sup = new NullSegment();

                    if (index + numTokens < _tokens.Count)
                    {
                        var tok2 = _tokens[index + numTokens];
                        if (tok2.TokenName == "command" && tok2.Value == "^")
                        {
                            var s = SegmentAt(index + numTokens + 1, out n);
                            if (s != null)
                            {
                                sup = s;
                                numTokens += n + 1;
                            }
                        }
                    }

                    return new SupSub(sup, sub);
                }
            }
            if (tok.TokenName == "escape")
            {
                if (tok.Value == "\\")
                    return new Return();
            }
            if (tok.TokenName == "whitespace")
                return new NullSegment();

            return new NullSegment(_tokens[index].Value.Select(c => new GlyphSegment(CmFont.Serif, c)));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public abstract class Segment
    {
        public List<Segment> Contents { get; } = new List<Segment>();

        public abstract double Width { get; }
        public abstract double Height { get; }

        public double Size { get; set; } = 1;
        public Point Offset { get; set; } = new Point(0, 0);

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
                    if (seg is Return)
                    {
                        o_x = 0;
                        o_y -= Size;
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

    public class GlyphSegment : Segment
    {
        private GlyphDescriptor _glyphDescriptor;
        public override double Width => _glyphDescriptor.AdvanceWidth * Size;
        public override double Height => _glyphDescriptor.AdvanceHeight * Size;

        public int CharId { get; }

        public char Char => (char) CharId;

        public GlyphSegment(CmFont font, int charId)
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
    }

    public class Return : Segment
    {
        public override double Width => 0;
        public override double Height => 0;
    }

    // todo: should be able to implement Return through latexspace.
    public class Space : Segment
    {
        public override double Width { get; }
        public override double Height { get; }

        public Space(double width, double height = 0)
        {
            Width = width;
            Height = height;
        }
    }

    public class NullSegment : Segment
    {
        public override double Width => Contents.Sum(ls => ls.Width);
        public override double Height => Contents.Max(ls => ls.Height);

        public NullSegment()
        {
        }

        public NullSegment(IEnumerable<Segment> contents)
        {
            Contents.AddRange(contents);
        }
    }

    public class SupSub : Segment
    {
        public override double Width => Contents.Max(s => s.Width);
        public override double Height => Contents.Sum(s => s.Height) + .1;

        public SupSub(Segment super, Segment sub)
        {
            Contents.Add(super);
            Contents.Add(sub);
        }

        public override IEnumerable<GlyphDescriptor> GlyphDescriptors
        {
            get
            {
                foreach (var sup in Contents[0].GlyphDescriptors)
                {
                    var cp = sup;
                    cp.Offset.X += Offset.X;
                    cp.Offset.Y += 0.45;
                    cp.Size *= 0.7;
                    yield return cp;
                }
                foreach (var sub in Contents[1].GlyphDescriptors)
                {
                    var cp = sub;
                    cp.Offset.X += Offset.X;
                    cp.Offset.Y -= 0.2;
                    cp.Size *= 0.7;
                    yield return cp;
                }
            }
        }
    }
}