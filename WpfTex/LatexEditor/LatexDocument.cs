using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using LatexEditor.Fonts;

namespace LatexEditor
{
    [ContentProperty("Content")]
    public class LatexDocument : FrameworkElement
    {
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(string), typeof(LatexDocument),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender));

        [Category("LaTeX")]
        [Description("LaTeX markup")]
        public string Content
        {
            get => (string)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(LatexDocument),
                new FrameworkPropertyMetadata(20d, FrameworkPropertyMetadataOptions.AffectsRender));

        [Category("LaTeX")]
        [Description("LaTeX font scaling")]
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        protected List<GlyphInfo> Parse(string latex)
        {
            var x = 0d;
            var y = 0d;
            var font = CmFont.SerifItalic;
            var size = FontSize;

            var glyphs = new List<GlyphInfo>(latex.Length);

            var state = new Stack<string>();
            state.Push("-");

            while (latex.Length > 0)
            {
                var s = state.Peek();

                if (s == "cmd")
                {
                    state.Pop();
                    var m = Regex.Match(latex, @"([a-z]+)(?=[^a-z]|$)", RegexOptions.IgnoreCase);
                    if (m.Index == 0)
                    {
                        var cmd = latex.Substring(0, m.Groups[1].Length);
                        switch (cmd)
                        {
                            case "it":
                                font = CmFont.SerifItalic;
                                break;
                            case "sym":
                                font = CmFont.Symbols;
                                break;
                            case "rm":
                                font = CmFont.Serif;
                                break;
                            case "sup":
                                size = FontSize * 0.4;
                                y = FontSize * (1 - 0.4);
                                break;
                            case "sub":
                                size = FontSize * 0.5;
                                y = -FontSize * 0.5 * 0.5;
                                break;
                            default:
                                continue;
                        }
                        latex = latex.Substring(m.Length).TrimStart(' ');
                    }
                    else
                    {
                        latex = latex.Substring(1);
                    }
                }
                else
                {
                    if (latex.StartsWith("\\"))
                    {
                        latex = latex.Substring(1);

                        state.Push("cmd");
                    }
                    else
                    {
                        var chr = latex[0];
                        latex = latex.Substring(1);

                        var gtf = font.GlyphTypeface();
                        var point = new Point(x, y);
                        var gi = new GlyphInfo(font, chr, size, point);
                        x += gtf.AdvanceWidths[gi.Index] * size;
                        glyphs.Add(gi);
                    }
                }
            }

            return glyphs;
        }

        protected override void OnRender(DrawingContext dc)
        {
            var text = Content;

            if (string.IsNullOrEmpty(text)) return;

            var glyphInfoList = Parse(text);

            var gtfGroups = glyphInfoList.GroupBy(gi => gi.Gtf);
            foreach (var gtfGroup in gtfGroups)
            {
                var gtf = gtfGroup.Key;

                var sizeGroups = gtfGroup.GroupBy(gi => gi.Size);
                foreach (var sizeGroup in sizeGroups)
                {
                    var size = sizeGroup.Key;

                    var glyphs = sizeGroup.Select(gi => gi.Index).ToList();
                    var advanceWidths = new double[glyphs.Count];
                    var offsets = sizeGroup.Select(gi => gi.Offset).ToList();
                    var gr = new GlyphRun(gtf, 0, false, size,
                        glyphs, new Point(0, FontSize), advanceWidths,
                        offsets, null, null, null, null, null);
                    dc.DrawGlyphRun(Brushes.Black, gr);
                }
            }
        }
    }
}
