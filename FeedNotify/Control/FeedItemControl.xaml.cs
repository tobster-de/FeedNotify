// FeedItemControl.xaml.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FeedNotify.Data;

namespace FeedNotify.Control
{
    /// <summary>
    ///     Interaction logic for FeedItemControl.xaml
    /// </summary>
    public partial class FeedItemControl : UserControl, INotifyPropertyChanged
    {
        private int timeout, initialTimeout;

        private Timer timer;

        #region Static Fields

        public static readonly DependencyProperty OpenCommandProperty = DependencyProperty.RegisterAttached(
            "OpenCommand",
            typeof(ICommand),
            typeof(FeedItemControl),
            new PropertyMetadata(null));

        public static readonly DependencyProperty TimeoutCommandProperty = DependencyProperty.RegisterAttached(
            "TimeoutCommand",
            typeof(ICommand),
            typeof(FeedItemControl),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ItemProperty = DependencyProperty.RegisterAttached(
            "Item",
            typeof(FeedItem),
            typeof(FeedItemControl),
            new PropertyMetadata(null));

        public static readonly DependencyProperty TimeoutProperty = DependencyProperty.RegisterAttached(
            "Timeout",
            typeof(int),
            typeof(FeedItemControl),
            new PropertyMetadata(-1));

        public static readonly DependencyProperty FilterTextProperty = DependencyProperty.RegisterAttached(
            "FilterText",
            typeof(string),
            typeof(FeedItemControl),
            new PropertyMetadata(null));

        #endregion

        #region Constructors and Destructors

        public FeedItemControl()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public ICommand OpenCommand
        {
            get => (ICommand)this.GetValue(FeedItemControl.OpenCommandProperty);

            set
            {
                this.SetValue(FeedItemControl.OpenCommandProperty, value);
                this.OnPropertyChanged();
            }
        }

        public ICommand TimeoutCommand
        {
            get => (ICommand)this.GetValue(FeedItemControl.TimeoutCommandProperty);

            set
            {
                this.SetValue(FeedItemControl.TimeoutCommandProperty, value);
                this.OnPropertyChanged();
            }
        }

        public FeedItem Item
        {
            get => (FeedItem)this.GetValue(FeedItemControl.ItemProperty);

            set
            {
                this.SetValue(FeedItemControl.ItemProperty, value);
                this.OnPropertyChanged();
            }
        }

        public int Timeout
        {
            get => (int)this.GetValue(FeedItemControl.TimeoutProperty);

            set
            {
                this.SetValue(FeedItemControl.TimeoutProperty, value);
                this.OnPropertyChanged();
            }
        }

        public string FilterText
        {
            get => (string)this.GetValue(FeedItemControl.FilterTextProperty);

            set
            {
                this.SetValue(FeedItemControl.FilterTextProperty, value);
                this.OnPropertyChanged();
            }
        }

        public bool UseTimeout => this.Timeout >= 0 && this.TimeoutCommand != null;

        public double TimeoutPercentage => this.UseTimeout && this.initialTimeout > 0 ? this.timeout * 100 / this.initialTimeout : 100;

        #endregion

        #region Methods

        private void TimingStep()
        {
            this.timeout -= 100;
            this.OnPropertyChanged(nameof(this.TimeoutPercentage));

            if (this.timeout <= 0)
            {
                this.Dispatcher.Invoke(() => this.TimeoutCommand?.Execute(this.Item));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Left)
            {
                this.OpenCommand?.Execute(this.Item);
            }
        }

        #endregion

        private void TimeoutControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.UseTimeout)
            {
                this.timeout = this.initialTimeout;
                this.OnPropertyChanged(nameof(this.TimeoutPercentage));
            }
        }

        private void feedItemControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.timer != null)
            {
                this.timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                this.timer.Dispose();
                this.timer = null;
            }
        }

        private void feedItemControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.UseTimeout)
            {
                this.OnPropertyChanged(nameof(this.UseTimeout));

                this.timeout = this.Timeout;
                this.initialTimeout = this.Timeout;

                this.timer = new Timer(o => this.TimingStep(), null, 0, 100);
            }
        }
    }
}