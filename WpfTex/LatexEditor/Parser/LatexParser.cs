using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Navigation;
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

        internal static readonly Dictionary<string, int> GreekLetters = new Dictionary<string, int>
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

            // Alpha Beta Epsilon Zeta Eta Iota Kappa Mu Nu 
            // Omicron Rho Tau and Upsilon all denoted by
            // Latin symbols A B E Z H I K M N O P T and Y
            ["Gamma"] = 0x0393,
            ["Delta"] = 0x0394,
            ["Theta"] = 0x0398,
            ["Lambda"] = 0x039b,
            ["Xi"] = 0x039e,
            ["Pi"] = 0x03a0,
            ["Sigma"] = 0x03a3,
            ["Phi"] = 0x03a6,
            ["Psi"] = 0x03a8,
            ["Omega"] = 0x03a9,
        };

        internal static readonly Dictionary<string, double> Spaces = new Dictionary<string, double>
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

        public static IEnumerable<GlyphInfo> ToGlyphInfos(string latex) => ToGlyphInfos(LatexLexer.Tokenize(latex));

        private static IEnumerable<GlyphInfo> ToGlyphInfos(IEnumerable<Token> tokens)
        {
            var lss = LatexSegment.ToLatexSegment(tokens);
            return lss.Glyphs;
        }
    }
}