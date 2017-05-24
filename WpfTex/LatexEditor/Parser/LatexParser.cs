using System.Collections.Generic;
using System.Windows;
using LatexEditor.Fonts;

namespace LatexEditor.Parser
{
    public static class LatexParser
    {
        private static readonly Lexer LatexLexer = new Lexer()
        {
            new TokenDescriptor("whitespace", @"\s+"),
            new TokenDescriptor("escape", @"\\([\#\$\%\^\&_\{\}\~\\])", 1),
            new TokenDescriptor("command", @"\\([^\d\s\\]+|[\ ])", 1),
            new TokenDescriptor("command", @"[\#\$\%\^\&_]"),
            new TokenDescriptor("number", @"\d"),
            new TokenDescriptor("open", @"\{"),
            new TokenDescriptor("close", @"\}"),
            new TokenDescriptor("letter", @"[\S]"), // nearly catch-all for uncaptured symbols
        };

        private static readonly Dictionary<string, int> greekLetters = new Dictionary<string, int>
        {
            ["alpha"] = 0x03b1,
            ["beta"] = 0x03b2,
            ["gamma"] = 0x03b3,
            ["delta"] = 0x03b4,
            ["epsilon"] = 0x03b5,
            ["zeta"] = 0x03b6,
            ["eta"] = 0x03b7,
            ["theta"] = 0x03b8,
            ["iota"] = 0x03b9,
            ["kappa"] = 0x03ba,
            ["lambda"] = 0x03bb,
            ["mu"] = 0x03bc,
            ["nu"] = 0x03bd,
            ["xi"] = 0x03be,
            ["omicron"] = 0x03bf,
            ["pi"] = 0x03c0,
            ["rho"] = 0x03c1,
            ["sigma"] = 0x03c2,
            ["tau"] = 0x03c4,
            ["upsilon"] = 0x03c5,
            ["phi"] = 0x03c6,
            ["chi"] = 0x03c7,
            ["psi"] = 0x03c8,
            ["omega"] = 0x03c9,

            ["Gamma"]   = 0x0393,
            ["Delta"]   = 0x0394,
            ["Theta"]   = 0x0398,
            ["Lambda"]  = 0x039b,
            ["Xi"]      = 0x039e,
            ["Pi"]      = 0x03a0,
            ["Sigma"]   = 0x03a3,
            ["Phi"]     = 0x03a6,
            ["Psi"]     = 0x03a8,
            ["Omega"]   = 0x03a9,
        };

        private static readonly Dictionary<string, double> spaces = new Dictionary<string, double>
        {
            [","] = 0.16666,
            ["!"] = -0.16666,
            [">"] = .3,
            [":"] = .3,
            [";"] = .4,
            [" "] = .5,
            ["quad"] = 1,
            ["qquad"] = 4,
        };

        public static IEnumerable<GlyphInfo> Parse(string latex) => Parse(LatexLexer.Tokenize(latex));

        private static IEnumerable<GlyphInfo> Parse(IEnumerable<Token> tokens)
        {
            var tokenQ = new Queue<Token>(tokens);
            var glyphs = new List<GlyphInfo>();

            const double size = 20;
            var x = 0d;
            var y = 0d;

            void AddGlyph(CmFont font, int c)
            {
                glyphs.Add(new GlyphInfo(font, c, size, new Point(x * size, y * size)));
                var gtf = font.GlyphTypeface();
                var idx = gtf.CharacterToGlyphMap[c];
                x += gtf.AdvanceWidths[idx];
            }

            while (tokenQ.Count > 0)
            {
                var token = tokenQ.Dequeue();
                var val = token.Value;
                switch (token.TokenName)
                {
                    case "letter":
                        AddGlyph(CmFont.SerifItalic, val[0]);
                        break;
                    case "number":
                        AddGlyph(CmFont.Serif, val[0]);
                        break;
                    case "command":
                        if (greekLetters.ContainsKey(val))
                            AddGlyph(CmFont.SerifItalic, greekLetters[val]);
                        else if (spaces.ContainsKey(val))
                            x += spaces[val];
                        else
                            foreach (var c in token.CapturedString)
                                AddGlyph(CmFont.SerifItalic, c);
                        break;
                    case "escape":
                        if (val == "\\")
                        {
                            x = 0;
                            y -= 1;
                        }
                        break;
                }
            }

            return glyphs;
        }
    }
}