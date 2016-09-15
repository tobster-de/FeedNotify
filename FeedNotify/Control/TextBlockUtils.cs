// TextBlockUtils.cs
using System.Windows;
using System.Windows.Controls;

namespace FeedNotify.Control
{
    public static class TextBlockUtils
    {
        #region Static Fields

        public static readonly DependencyProperty AutoTooltipProperty =
            DependencyProperty.RegisterAttached(
                "AutoTooltip",
                typeof(bool),
                typeof(TextBlockUtils),
                new PropertyMetadata(false, OnAutoTooltipPropertyChanged));

        #endregion

        #region Public Methods and Operators

        public static bool GetAutoTooltip(DependencyObject d)
        {
            return (bool)d.GetValue(AutoTooltipProperty);
        }

        public static void SetAutoTooltip(DependencyObject d, bool value)
        {
            d.SetValue(AutoTooltipProperty, value);
        }

        #endregion

        #region Methods

        private static void OnAutoTooltipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = d as TextBlock;
            if (tb != null)
            {
                var newValue = (bool)e.NewValue;
                if (newValue)
                {
                    SetTooltipBasedOnTrimmingState(tb);
                    tb.SizeChanged += OnTextBlockSizeChanged;
                }
                else
                {
                    tb.SizeChanged -= OnTextBlockSizeChanged;
                }
            }
        }

        private static void OnTextBlockSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var tb = sender as TextBlock;
            if (tb != null)
            {
                SetTooltipBasedOnTrimmingState(tb);
            }
        }

        private static void SetTooltipBasedOnTrimmingState(TextBlock tb)
        {
            tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var isTextTrimmed = tb.ActualWidth < tb.DesiredSize.Width;
            ToolTipService.SetToolTip(tb, isTextTrimmed ? tb.Text : null);
        }

        #endregion
    }
}