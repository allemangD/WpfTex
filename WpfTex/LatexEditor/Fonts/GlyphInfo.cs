using System.Windows;
using System.Windows.Media;

namespace LatexEditor.Fonts
{
    public class GlyphInfo
    {
        public CmFont Font;
        public double RelativeSize;
        public int Char;
        public Point RelativeOffset;
        public Point BaselineOrigin;

        public GlyphTypeface Gtf => Font.GlyphTypeface();

        public ushort Index => Gtf.CharacterToGlyphMap[Char];

        public double RelAdvWidth => Gtf.AdvanceWidths[Index];
        public double RelAdvHeight => Gtf.AdvanceHeights[Index];

        public Point Offset => new Point(
            BaselineOrigin.X + RelativeOffset.X * RelativeSize,
            BaselineOrigin.Y + RelativeOffset.Y * RelativeSize);

        public GlyphInfo(CmFont font, int c, Point relativeOffset, Point baselineOrigin, double relativeSize = 1)
        {
            Font = font;
            RelativeSize = relativeSize;
            RelativeOffset = relativeOffset;
            BaselineOrigin = baselineOrigin;
            Char = c;
        }
    }
}