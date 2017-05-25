using System.Collections.Generic;
using System.Linq;
using LatexEditor.Fonts;

namespace LatexEditor.Parser.Segments
{
    public class LatexRun : LatexSegment
    {
        public override IEnumerable<GlyphInfo> Glyphs => Segments.SelectMany(ls => ls.Glyphs);

        public override double RelAdvWidth { get; }
        public override double RelAdvHeight { get; }

        public IEnumerable<LatexSegment> Segments { get; set; }

        public LatexRun(IEnumerable<LatexSegment> segments)
        {
            Segments = segments;

            // to shift characters into position
            var x = 0d;
            var y = 0d;
            foreach (var segment in Segments)
            {
                if (segment is LatexReturn)
                {
                    x = 0d;
                    y -= segment.RelAdvHeight;
                    continue;
                }
                foreach (var gi in segment.Glyphs)
                {
                    gi.BaselineOrigin.X = x;
                    gi.BaselineOrigin.Y = y;
                }
                x += segment.RelAdvWidth;
            }
            RelAdvWidth = x;
        }
    }
}