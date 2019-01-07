// MainWindow.xaml.cs
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace FeedNotify
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructors and Destructors

        public MainWindow()
        {
            this.InitializeComponent();

            this.Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentCulture.Name);
        }

        #endregion

        //private void Window_Closed(object sender, System.EventArgs e) => Application.Current.Shutdown();
    }
}