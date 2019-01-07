// NotificationWindow.xaml.cs
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using FeedNotify.Data;
using FeedNotify.ViewModel;

namespace FeedNotify.View
{
    /// <summary>
    ///     Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : Window
    {
        #region Fields

        private Taskbar taskbar;

        #endregion

        #region Constructors and Destructors

        public NotificationWindow()
        {
            this.InitializeComponent();

            this.Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentCulture.Name);
        }

        #endregion

        #region Methods

        internal void DisplayItems(IEnumerable<FeedItem> displayItems)
        {
            ((NotificationWindowViewModel)this.DataContext).SetDisplayItems(displayItems);
        }

        private void SetPosition()
        {
            if (this.taskbar == null)
            {
                this.taskbar = new Taskbar();
            }
            if (this.taskbar.Position == TaskbarPosition.Right)
            {
                this.Left = SystemParameters.PrimaryScreenWidth - this.taskbar.Size.Width - this.ActualWidth;
                this.Top = SystemParameters.PrimaryScreenHeight - this.ActualHeight;
            }
            else
            {
                this.Left = SystemParameters.PrimaryScreenWidth - this.ActualWidth;
                this.Top = SystemParameters.PrimaryScreenHeight - this.ActualHeight;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.SetPosition();
        }

        #endregion
    }
}