using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using LatexEditor.Parser;
using LatexEditor.Parser.Segments;

namespace LatexEditor
{
    [ContentProperty("Content")]
    public class LatexViewer : FrameworkElement
    {
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(string), typeof(LatexViewer),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender));

        [Category("LaTeX")]
        [Description("LaTeX markup")]
        public string Content
        {
            get => (string) GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(LatexViewer),
                new FrameworkPropertyMetadata(20d, FrameworkPropertyMetadataOptions.AffectsRender));

        [Category("LaTeX")]
        [Description("LaTeX font scaling")]
        public double FontSize
        {
            get => (double) GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (string.IsNullOrEmpty(Content)) return;

//            var seg = Segment.ToLatexSegment(LatexParser.Tokenize(Content));

            var seg = new Run(new SegmentCollection(LatexParser.Tokenize(Content)));

            var gds = seg.GlyphDescriptors.ToList();

            var tfGroups = gds.GroupBy(gd => gd.Typeface);
            foreach (var tfGroup in tfGroups)
            {
                var tf = tfGroup.Key;

                var sizeGroups = tfGroup.GroupBy(gd => gd.Size);
                foreach (var sizeGroup in sizeGroups)
                {
                    var size = sizeGroup.Key;

                    var glyphs = sizeGroup.Select(gd => gd.Index).ToList();
                    var advWidths = new double[glyphs.Count]; // base only on offset
                    var offsets = sizeGroup.Select(gd => gd.Offset)
                        .Select(o => new Point(o.X * FontSize, o.Y * FontSize))
                        .ToList();
                    var gr = new GlyphRun(
                        tf, 0, false, size * FontSize,
                        glyphs, new Point(0, FontSize), advWidths,
                        offsets, null, null, null, null, null
                    );

                    dc.DrawGlyphRun(Brushes.Black, gr);
                }
            }
        }
    }
}