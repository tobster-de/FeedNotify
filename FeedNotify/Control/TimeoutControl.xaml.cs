using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FeedNotify.Control
{
    /// <summary>
    /// Interaction logic for TimeoutControl.xaml
    /// </summary>
    public partial class TimeoutControl : UserControl
    {
        /// <summary>
        /// Interaction logic for CircularProgressBar.xaml
        /// </summary>
        public TimeoutControl()
        {
            this.InitializeComponent();
            this.Angle = (this.Percentage * 360) / 100;
            this.RenderArc();
        }

        public int Radius
        {
            get { return (int)this.GetValue(RadiusProperty); }
            set {
                this.SetValue(RadiusProperty, value); }
        }

        public Brush SegmentColor
        {
            get { return (Brush)this.GetValue(SegmentColorProperty); }
            set {
                this.SetValue(SegmentColorProperty, value); }
        }

        public int StrokeThickness
        {
            get { return (int)this.GetValue(StrokeThicknessProperty); }
            set {
                this.SetValue(StrokeThicknessProperty, value); }
        }

        public double Percentage
        {
            get { return (double)this.GetValue(PercentageProperty); }
            set {
                this.SetValue(PercentageProperty, value); }
        }

        public double Angle
        {
            get { return (double)this.GetValue(AngleProperty); }
            set {
                this.SetValue(AngleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Percentage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register("Percentage", typeof(double), typeof(TimeoutControl), new PropertyMetadata(65d, new PropertyChangedCallback(OnPercentageChanged)));

        // Using a DependencyProperty as the backing store for StrokeThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(int), typeof(TimeoutControl), new PropertyMetadata(5));

        // Using a DependencyProperty as the backing store for SegmentColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SegmentColorProperty =
            DependencyProperty.Register("SegmentColor", typeof(Brush), typeof(TimeoutControl), new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(int), typeof(TimeoutControl), new PropertyMetadata(25, new PropertyChangedCallback(OnPropertyChanged)));

        // Using a DependencyProperty as the backing store for Angle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(TimeoutControl), new PropertyMetadata(120d, new PropertyChangedCallback(OnPropertyChanged)));

        private static void OnPercentageChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            TimeoutControl circle = sender as TimeoutControl;
            circle.Angle = (circle.Percentage * 360) / 100;
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            TimeoutControl circle = sender as TimeoutControl;
            circle.RenderArc();
        }

        public void RenderArc()
        {
            Point startPoint = new Point(this.Radius, 0);
            Point endPoint = this.ComputeCartesianCoordinate(this.Angle, this.Radius);
            endPoint.X += this.Radius;
            endPoint.Y += this.Radius;

            this.pathRoot.Width = this.Radius * 2 + this.StrokeThickness;
            this.pathRoot.Height = this.Radius * 2 + this.StrokeThickness;
            this.pathRoot.Margin = new Thickness(this.StrokeThickness, this.StrokeThickness, 0, 0);

            bool largeArc = this.Angle > 180.0;

            Size outerArcSize = new Size(this.Radius, this.Radius);

            this.pathFigure.StartPoint = startPoint;

            if (startPoint.X == Math.Round(endPoint.X) && startPoint.Y == Math.Round(endPoint.Y))
                endPoint.X -= 0.01;

            this.arcSegment.Point = endPoint;
            this.arcSegment.Size = outerArcSize;
            this.arcSegment.IsLargeArc = largeArc;
        }

        private Point ComputeCartesianCoordinate(double angle, double radius)
        {
            // convert to radians
            double angleRad = (Math.PI / 180.0) * (angle - 90);

            double x = radius * Math.Cos(angleRad);
            double y = radius * Math.Sin(angleRad);

            return new Point(x, y);
        }
    }
}