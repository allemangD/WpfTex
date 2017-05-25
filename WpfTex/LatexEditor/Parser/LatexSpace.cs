using System.Collections.Generic;
using System.Linq;
using LatexEditor.Fonts;

namespace LatexEditor.Parser
{
    public class LatexSpace : LatexSegment
    {
        public override IEnumerable<GlyphInfo> Glyphs => Enumerable.Empty<GlyphInfo>();
        public override double RelAdvWidth { get; }
        public override double RelAdvHeight { get; }

        public LatexSpace(double relAdvWidth)
        {
            RelAdvWidth = relAdvWidth;
            RelAdvHeight = 0;
        }
    }
}