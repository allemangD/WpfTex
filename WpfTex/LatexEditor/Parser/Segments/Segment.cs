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
                var seg = new Run();
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

                    Segment sub = new Run();

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

                    Segment sup = new Run();

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
                return new Run();

            return new Run(_tokens[index].Value.Select(c => new GlyphSegment(CmFont.Serif, c)));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public abstract class Segment
    {
        public abstract double Width { get; }
        public abstract double Height { get; }

        public double Size { get; set; } = 1;
        public Point Offset { get; set; } = new Point(0, 0);

        public override string ToString()
        {
            return GetType().Name;
        }

        protected GlyphDescriptor Localized(GlyphDescriptor gd) => Localized(gd, 0, 0);

        protected GlyphDescriptor Localized(GlyphDescriptor gd, double x, double y)
        {
            // value type, just mutate the parameter
            gd.Size *= Size;
            gd.Offset.X += Offset.X + x;
            gd.Offset.Y += Offset.Y + y;
            return gd;
        }

        public abstract IEnumerable<GlyphDescriptor> GlyphDescriptors { get; }
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
            get { yield return Localized(_glyphDescriptor); }
        }
    }

    // todo: should be able to implement Return through Space.
    public class Return : Segment
    {
        public override double Width => 0;
        public override double Height => 0;
        public override IEnumerable<GlyphDescriptor> GlyphDescriptors => Enumerable.Empty<GlyphDescriptor>();
    }

    public class Space : Segment
    {
        public override double Width { get; }
        public override double Height { get; }
        public override IEnumerable<GlyphDescriptor> GlyphDescriptors => Enumerable.Empty<GlyphDescriptor>();

        public Space(double width, double height = 0)
        {
            Width = width;
            Height = height;
        }
    }

    public class SupSub : Segment
    {
        public Segment Super { get; set; }
        public Segment Sub { get; set; }
        public override double Width => Math.Max(Super.Width, Sub.Width);
        public override double Height => Super.Height + Sub.Height + .1;

        public SupSub(Segment super, Segment sub)
        {
            Super = super;
            Super.Offset = new Point(0, 0.45);
            Super.Size = 0.7;

            Sub = sub;
            Sub.Offset = new Point(0, -0.2);
            Sub.Size = 0.7;
        }

        public override IEnumerable<GlyphDescriptor> GlyphDescriptors
        {
            get
            {
                foreach (var gd in Super.GlyphDescriptors)
                    yield return Localized(gd);

                foreach (var gd in Sub.GlyphDescriptors)
                    yield return Localized(gd);
            }
        }
    }

    public class Run : Segment
    {
        public List<Segment> Contents;
        public override double Width => Contents.Sum(ls => ls.Width);
        public override double Height => Contents.Max(ls => ls.Height);

        public Run(IEnumerable<Segment> contents = null)
        {
            Contents = contents?.ToList() ?? new List<Segment>();
        }

        public override IEnumerable<GlyphDescriptor> GlyphDescriptors
        {
            get
            {
                var x = 0d;
                var y = 0d;

                foreach (var seg in Contents)
                {
                    if (seg is Return)
                    {
                        x = 0;
                        y -= 1;
                        continue;
                    }
                    foreach (var gd in seg.GlyphDescriptors)
                        yield return Localized(gd, x, y);

                    x += seg.Width;
                }
            }
        }
    }
}