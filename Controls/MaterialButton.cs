using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MAS_GUI.Controls
{
    public class MaterialButton : Button
    {
        private FrameworkElement ripple;
        private FrameworkElement rippleHost;
        private TranslateTransform rippleTranslate;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ripple = GetTemplateChild("PART_Ripple") as FrameworkElement;
            rippleHost = GetTemplateChild("PART_RippleHost") as FrameworkElement;
            rippleTranslate = GetTemplateChild("PART_RippleTranslate") as TranslateTransform;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (ripple == null || rippleTranslate == null)
            {
                return;
            }

            FrameworkElement host = rippleHost ?? (FrameworkElement)this;
            Point position = e.GetPosition(host);
            double width = host.ActualWidth;
            double height = host.ActualHeight;
            if (width <= 0 || height <= 0)
            {
                width = ripple.ActualWidth;
                height = ripple.ActualHeight;
            }
            rippleTranslate.X = position.X - width / 2.0;
            rippleTranslate.Y = position.Y - height / 2.0;
        }
    }
}
