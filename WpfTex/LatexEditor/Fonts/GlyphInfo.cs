using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace LatexEditor.Fonts
{
    public class GlyphInfo
    {
        public CmFont Font;
        public Point Offset;
        public double Size;
        private int Char;

        public GlyphTypeface Gtf => Font.GlyphTypeface();

        /// <summary>
        /// Index of this glyph within the GlyphTypeface. 
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if <see cref="Font"/> does not have a glyph for char <see cref="Char"/></exception>
        public ushort Index => Gtf.CharacterToGlyphMap[Char];

        public GlyphInfo(CmFont font, int character, double size, Point offset)
        {
            Font = font;
            Char = character;
            Size = size;
            Offset = offset;
        }
    }
}