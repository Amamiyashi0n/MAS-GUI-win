using System;
using System.Windows;
using System.Windows.Media;

namespace MAS_GUI.Controls
{
    public class MaterialProgressBar : FrameworkElement
    {
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(
                "Progress",
                typeof(double),
                typeof(MaterialProgressBar),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register(
                "IsIndeterminate",
                typeof(bool),
                typeof(MaterialProgressBar),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TrackBrushProperty =
            DependencyProperty.Register(
                "TrackBrush",
                typeof(Brush),
                typeof(MaterialProgressBar),
                new FrameworkPropertyMetadata(Brushes.LightGray, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IndicatorBrushProperty =
            DependencyProperty.Register(
                "IndicatorBrush",
                typeof(Brush),
                typeof(MaterialProgressBar),
                new FrameworkPropertyMetadata(Brushes.MediumPurple, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IndeterminateOffsetProperty =
            DependencyProperty.Register(
                "IndeterminateOffset",
                typeof(double),
                typeof(MaterialProgressBar),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty IndeterminateWidthProperty =
            DependencyProperty.Register(
                "IndeterminateWidth",
                typeof(double),
                typeof(MaterialProgressBar),
                new FrameworkPropertyMetadata(0.35, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register(
                "Angle",
                typeof(double),
                typeof(MaterialProgressBar),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty SweepAngleProperty =
            DependencyProperty.Register(
                "SweepAngle",
                typeof(double),
                typeof(MaterialProgressBar),
                new FrameworkPropertyMetadata(270.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register(
                "Thickness",
                typeof(double),
                typeof(MaterialProgressBar),
                new FrameworkPropertyMetadata(4.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public bool IsIndeterminate
        {
            get { return (bool)GetValue(IsIndeterminateProperty); }
            set { SetValue(IsIndeterminateProperty, value); }
        }

        public Brush TrackBrush
        {
            get { return (Brush)GetValue(TrackBrushProperty); }
            set { SetValue(TrackBrushProperty, value); }
        }

        public Brush IndicatorBrush
        {
            get { return (Brush)GetValue(IndicatorBrushProperty); }
            set { SetValue(IndicatorBrushProperty, value); }
        }

        public double IndeterminateOffset
        {
            get { return (double)GetValue(IndeterminateOffsetProperty); }
            set { SetValue(IndeterminateOffsetProperty, value); }
        }

        public double IndeterminateWidth
        {
            get { return (double)GetValue(IndeterminateWidthProperty); }
            set { SetValue(IndeterminateWidthProperty, value); }
        }

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public double SweepAngle
        {
            get { return (double)GetValue(SweepAngleProperty); }
            set { SetValue(SweepAngleProperty, value); }
        }

        public double Thickness
        {
            get { return (double)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double size = double.IsNaN(Width) ? 32 : Width;
            if (!double.IsNaN(Height))
            {
                size = Math.Min(size, Height);
            }
            if (!double.IsNaN(availableSize.Width) && availableSize.Width > 0)
            {
                size = Math.Min(size, availableSize.Width);
            }
            if (!double.IsNaN(availableSize.Height) && availableSize.Height > 0)
            {
                size = Math.Min(size, availableSize.Height);
            }
            return new Size(size, size);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            double width = ActualWidth;
            double height = ActualHeight;
            if (width <= 0 || height <= 0) return;

            double size = Math.Min(width, height);
            double radius = Math.Max(0, (size - Thickness) / 2.0);
            if (radius <= 0) return;

            Point center = new Point(width / 2.0, height / 2.0);
            Pen trackPen = new Pen(TrackBrush, Thickness);
            drawingContext.DrawEllipse(null, trackPen, center, radius, radius);

            double sweep = IsIndeterminate ? SweepAngle : Math.Max(0.0, Math.Min(1.0, Progress)) * 360.0;
            if (sweep <= 0.1) return;

            double startAngle = Angle;
            double endAngle = startAngle + sweep;
            bool isLargeArc = sweep > 180.0;

            Point start = GetPointOnCircle(center, radius, startAngle);
            Point end = GetPointOnCircle(center, radius, endAngle);

            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(start, false, false);
                ctx.ArcTo(end, new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise, true, false);
            }
            geometry.Freeze();

            Pen indicatorPen = new Pen(IndicatorBrush, Thickness);
            indicatorPen.StartLineCap = PenLineCap.Round;
            indicatorPen.EndLineCap = PenLineCap.Round;
            drawingContext.DrawGeometry(null, indicatorPen, geometry);
        }

        private static Point GetPointOnCircle(Point center, double radius, double angle)
        {
            double rad = angle * Math.PI / 180.0;
            return new Point(center.X + radius * Math.Cos(rad), center.Y + radius * Math.Sin(rad));
        }
    }
}
