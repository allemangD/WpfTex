using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

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
            Debug.WriteLine(dc, nameof(LatexViewer));
            Debug.WriteLine(dc.GetType(), nameof(LatexViewer));

            if (string.IsNullOrEmpty(Content)) return;

            var glyphInfoList = Parser.Parser.ToGlyphInfos(Content);

            var gtfGroups = glyphInfoList.GroupBy(gi => gi.Gtf);
            foreach (var gtfGroup in gtfGroups)
            {
                var gtf = gtfGroup.Key;

                var sizeGroups = gtfGroup.GroupBy(gi => gi.RelativeSize);
                foreach (var sizeGroup in sizeGroups)
                {
                    var size = sizeGroup.Key;

                    var glyphs = sizeGroup.Select(gi => gi.Index).ToList();
                    var advanceWidths = new double[glyphs.Count]; // all zero - position based only on offset
                    var offsets = sizeGroup.Select(gi => new Point(gi.Offset.X * FontSize, gi.Offset.Y * FontSize))
                        .ToList();
                    var gr = new GlyphRun(gtf, 0, false, size * FontSize,
                        glyphs, new Point(0, FontSize), advanceWidths,
                        offsets, null, null, null, null, null);

                    dc.DrawGlyphRun(Brushes.Black, gr);
                }
            }
        }
    }
}