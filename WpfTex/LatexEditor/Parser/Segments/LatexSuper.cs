using System.Collections.Generic;
using LatexEditor.Fonts;

namespace LatexEditor.Parser.Segments
{
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