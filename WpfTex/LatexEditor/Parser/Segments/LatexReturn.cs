using System.Collections.Generic;
using System.Linq;
using LatexEditor.Fonts;

namespace LatexEditor.Parser.Segments
{
    public class LatexReturn : LatexSegment
    {
        public override IEnumerable<GlyphInfo> Glyphs => Enumerable.Empty<GlyphInfo>();
        public override double RelAdvWidth => 0;
        public override double RelAdvHeight { get; }

        public LatexReturn(double relAdvHeight)
        {
            RelAdvHeight = relAdvHeight;
        }
    }
}