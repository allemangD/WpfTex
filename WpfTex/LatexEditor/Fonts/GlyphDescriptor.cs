using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace LatexEditor.Fonts
{
    public struct GlyphDescriptor
    {
        public GlyphTypeface Typeface;
        public ushort Index;
        public string CharacterName;
        public double Size;
        public Point Offset;

        public double AdvanceWidth => Typeface.AdvanceWidths[Index] * Size;
        public double AdvanceHeight => Typeface.AdvanceHeights[Index] * Size;
        public double Height => Typeface.Height * Size;

        public GlyphDescriptor(GlyphTypeface typeface, int character)
        {
            Typeface = typeface;
            Index = Typeface.CharacterToGlyphMap[character];
            CharacterName = ((char) character).ToString();
            Offset = new Point(0, 0);
            Size = 1;
        }

        public override string ToString()
        {
            return $"'{CharacterName}' ({Index}) {Offset} {AdvanceWidth}x{AdvanceHeight} [{Height}]";
        }
    }
}