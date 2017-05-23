using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LatexEditor.Fonts
{
    public static class CmManager
    {
        private static readonly Dictionary<CmFont, string> FontCodes = new Dictionary<CmFont, string>
        {
            [CmFont.Serif] = "cmun/cmunrm",
            [CmFont.SerifOblique] = "cmun/cmunsl",
            [CmFont.SerifItalic] = "cmun/cmunti",
            [CmFont.SerifItalicUpright] = "cmun/cmunui",
            [CmFont.SerifBold] = "cmun/cmunbx",
            [CmFont.SerifBoldOblique] = "cmun/cmunbl",
            [CmFont.SerifBoldItalic] = "cmun/cmunbi",
            [CmFont.ClassicalItalic] = "cmun/cmunci",
            [CmFont.Sans] = "cmun/cmunss",
            [CmFont.SansOblique] = "cmun/cmunsi",
            [CmFont.SansBold] = "cmun/cmunsx",
            [CmFont.SansBoldOblique] = "cmun/cmunso",
            [CmFont.SansDemicondensed] = "cmun/cmunssdc",
            [CmFont.Typewriter] = "cmun/cmuntt",
            [CmFont.TypewriterOblique] = "cmun/cmunst",
            [CmFont.TypewriterItalic] = "cmun/cmunit",
            [CmFont.TypewriterBold] = "cmun/cmuntb",
            [CmFont.TypewriterBoldItalic] = "cmun/cmuntx",
            [CmFont.TypewriterLight] = "cmun/cmunbtl",
            [CmFont.TypewriterLightOblique] = "cmun/cmunbto",
            [CmFont.TypewriterVariable] = "cmun/cmunvt",
            [CmFont.TypewriterVariableItalic] = "cmun/cmunvi",
            [CmFont.Concrete] = "cmun/cmunorm",
            [CmFont.ConcreteItalic] = "cmun/cmunoti",
            [CmFont.ConcreteBold] = "cmun/cmunobx",
            [CmFont.ConcreteBoldItalic] = "cmun/cmunobi",
            [CmFont.Bright] = "cmun/cmunbmr",
            [CmFont.BrightSemibold] = "cmun/cmunbmo",
            [CmFont.BrightSemiboldOblique] = "cmun/cmunbsr",
            [CmFont.BrightBold] = "cmun/cmunbbx",
            [CmFont.BrightBoldOblique] = "cmun/cmunbxo",

            [CmFont.MathItalic] = "cm/cmmi",
            [CmFont.MathBoldItalic] = "cm/cmmib",
            [CmFont.MathExtension] = "cm/cmex",
            [CmFont.Symbols] = "cm/cmsy",
            [CmFont.SymbolsBold] = "cm/cmbsy",
        };

        private static readonly Dictionary<CmFont, int[]> EmSizes = new Dictionary<CmFont, int[]>
        {
            [CmFont.MathItalic] = new[] { 5, 6, 7, 8, 9, 10, 12 },
            [CmFont.MathBoldItalic] = new[] { 6, 7, 8, 9, 10 },
            [CmFont.MathExtension] = new[] { 7, 8, 9, 10 },
            [CmFont.Symbols] = new[] { 5, 6, 7, 8, 9, 10 },
            [CmFont.SymbolsBold] = new[] { 6, 7, 8, 9, 10 }
        };

        private static readonly Dictionary<string, GlyphTypeface> Fonts = new Dictionary<string, GlyphTypeface>();

        public static GlyphTypeface GlyphTypeface(this CmFont font, double emSize = 10)
        {
            var code = FontCodes[font];
            if (EmSizes.ContainsKey(font))
            {
                // take font size which is closest to the em size
                var size = EmSizes[font]
                    .OrderBy(s => Math.Abs(s - emSize))
                    .First();
                code = code + size;
            }

            if (Fonts.ContainsKey(code)) return Fonts[code];

            var uri = new Uri("pack://application:,,,/LatexEditor;component/Fonts/" + code + ".ttf");
            return Fonts[code] = new GlyphTypeface(uri);
        }
    }
}
