using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MAS_GUI.Controls
{
    public partial class SponsorDialog : UserControl
    {
        public event EventHandler Closed;

        private bool _isClosing;
        private long _startTime;
        private bool _isAnimating;

        public SponsorDialog()
        {
            InitializeComponent();
        }

        public void Show()
        {
            Visibility = Visibility.Visible;
            Opacity = 0;
            CardScale.ScaleX = 0.8;
            CardScale.ScaleY = 0.8;
            
            _isClosing = false;
            StartAnimation();
        }

        public void Hide()
        {
            if (_isClosing) return;
            _isClosing = true;
            StartAnimation();
        }

        private void StartAnimation()
        {
            if (!_isAnimating)
            {
                _startTime = DateTime.Now.Ticks;
                CompositionTarget.Rendering += OnRendering;
                _isAnimating = true;
            }
            else
            {
                // Reset start time if already animating to reverse/restart gracefully
                _startTime = DateTime.Now.Ticks; 
            }
        }

        private void OnRendering(object sender, EventArgs e)
        {
            long now = DateTime.Now.Ticks;
            double elapsed = new TimeSpan(now - _startTime).TotalMilliseconds;
            
            // Duration: 300ms for enter, 200ms for exit
            double duration = _isClosing ? 200 : 300;
            double progress = Math.Min(1.0, elapsed / duration);

            // Apply Easing (Cubic Ease Out)
            // f(t) = 1 - (1-t)^3
            double eased = 1 - Math.Pow(1 - progress, 3);

            if (!_isClosing)
            {
                // Enter Animation
                Opacity = progress; // Linear opacity for enter
                double scale = 0.8 + (0.2 * eased);
                CardScale.ScaleX = scale;
                CardScale.ScaleY = scale;
            }
            else
            {
                // Exit Animation
                Opacity = 1 - progress; // Linear opacity fade out
                // Keep scale at 1 or slightly shrink? Let's just fade out.
            }

            if (progress >= 1.0)
            {
                CompositionTarget.Rendering -= OnRendering;
                _isAnimating = false;

                if (_isClosing)
                {
                    Visibility = Visibility.Collapsed;
                    if (Closed != null) Closed(this, EventArgs.Empty);
                }
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void OnOverlayClick(object sender, MouseButtonEventArgs e)
        {
            // Optional: Click outside to close
            // Hide();
        }

        private void OnCardClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
