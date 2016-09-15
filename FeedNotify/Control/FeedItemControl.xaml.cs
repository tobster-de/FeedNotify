// FeedItemControl.xaml.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        #region Static Fields

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(FeedItemControl),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ItemProperty = DependencyProperty.RegisterAttached(
            "Item",
            typeof(FeedItem),
            typeof(FeedItemControl),
            new PropertyMetadata(null));

        #endregion

        #region Constructors and Destructors

        //public string Title
        //{
        //    get
        //    {
        //        return (string)this.GetValue(TitleProperty);
        //    }

        //    set
        //    {
        //        this.SetValue(TitleProperty, value);
        //        this.OnPropertyChanged();
        //    }
        //}

        //public string Summary
        //{
        //    get
        //    {
        //        return (string)this.GetValue(SummaryProperty);
        //    }

        //    set
        //    {
        //        this.SetValue(SummaryProperty, value);
        //        this.OnPropertyChanged();
        //    }
        //}

        //public DateTime Publish
        //{
        //    get
        //    {
        //        return (DateTime)this.GetValue(PublishProperty);
        //    }

        //    set
        //    {
        //        this.SetValue(PublishProperty, value);
        //        this.OnPropertyChanged();
        //    }
        //}

        public FeedItemControl()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Public Events

        //public static readonly DependencyProperty TitleProperty = DependencyProperty.RegisterAttached(
        //    "Title",
        //    typeof(string),
        //    typeof(FeedItemControl),
        //    new PropertyMetadata(null));

        //public static readonly DependencyProperty SummaryProperty = DependencyProperty.RegisterAttached(
        //    "Summary",
        //    typeof(string),
        //    typeof(FeedItemControl),
        //    new PropertyMetadata(null));

        //public static readonly DependencyProperty PublishProperty = DependencyProperty.RegisterAttached(
        //    "Publish",
        //    typeof(DateTime),
        //    typeof(FeedItemControl),
        //    new PropertyMetadata(null));

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public ICommand Command
        {
            get
            {
                return (ICommand)this.GetValue(CommandProperty);
            }

            set
            {
                this.SetValue(CommandProperty, value);
                this.OnPropertyChanged();
            }
        }

        public FeedItem Item
        {
            get
            {
                return (FeedItem)this.GetValue(ItemProperty);
            }

            set
            {
                this.SetValue(ItemProperty, value);
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                this.Command?.Execute(this.Item);
            }
        }

        #endregion
    }
}