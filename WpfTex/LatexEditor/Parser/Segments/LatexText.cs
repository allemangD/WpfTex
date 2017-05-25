using System.Collections.Generic;
using System.Windows;
using LatexEditor.Fonts;

namespace LatexEditor.Parser.Segments
{
    public class LatexText : LatexSegment
    {
        private readonly List<GlyphInfo> _glyphs = new List<GlyphInfo>();

        public override IEnumerable<GlyphInfo> Glyphs => _glyphs;
        public override double RelAdvWidth { get; }
        public override double RelAdvHeight { get; }

        public LatexText(CmFont cmFont, int c)
        {
            var gi = new GlyphInfo(cmFont, c, new Point(0, 0), new Point(0, 0));
            _glyphs.Add(gi);
            RelAdvWidth = gi.RelAdvWidth;
            RelAdvHeight = gi.RelAdvHeight;
        }

        public LatexText(CmFont cmFont, string text)
        {
            var x = 0d;
            foreach (var c in text)
            {
                var gi = new GlyphInfo(cmFont, c, new Point(x, 0), new Point(0, 0));
                _glyphs.Add(gi);
                x += gi.RelAdvWidth;
            }
            RelAdvWidth = x;
        }
    }
}