using System;
using System.Windows;
using System.Windows.Media;

namespace MAS_GUI.Controls
{
    public class MaterialIcon : FrameworkElement
    {
        public static readonly DependencyProperty GeometryProperty =
            DependencyProperty.Register(
                "Geometry",
                typeof(Geometry),
                typeof(MaterialIcon),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register(
                "Fill",
                typeof(Brush),
                typeof(MaterialIcon),
                new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        public Geometry Geometry
        {
            get { return (Geometry)GetValue(GeometryProperty); }
            set { SetValue(GeometryProperty, value); }
        }

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double width = double.IsNaN(Width) ? 24 : Width;
            double height = double.IsNaN(Height) ? 24 : Height;
            return new Size(width, height);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (Geometry == null)
            {
                return;
            }

            Rect bounds = Geometry.Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            double targetWidth = double.IsNaN(Width) ? 24 : Width;
            double targetHeight = double.IsNaN(Height) ? 24 : Height;
            double scale = Math.Min(targetWidth / bounds.Width, targetHeight / bounds.Height);

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(scale, scale));
            transformGroup.Children.Add(new TranslateTransform(
                -bounds.X * scale + (targetWidth - bounds.Width * scale) / 2.0,
                -bounds.Y * scale + (targetHeight - bounds.Height * scale) / 2.0));

            drawingContext.PushTransform(transformGroup);
            drawingContext.DrawGeometry(Fill, null, Geometry);
            drawingContext.Pop();
        }
    }
}
