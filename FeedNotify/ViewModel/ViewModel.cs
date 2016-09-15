// ViewModel.cs
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using FeedNotify.Data;
using FeedNotify.View;

namespace FeedNotify.ViewModel
{
    internal class ViewModel : NotifyPropertyChanged
    {
        #region Fields

        private ActionCommand loadCommand;

        private NotificationWindow notificationWindow;

        private ICommand openCommand;

        private bool running;

        private Timer timer;

        #endregion

        #region Constructors and Destructors

        public ViewModel()
        {
            this.notificationWindow = new NotificationWindow();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            this.timer = new Timer(o => this.StartLoading(), null, 0, 1000 * 60 * 2);
        }

        #endregion

        #region Public Properties

        public ObservableCollection<FeedItem> FeedItems { get; } = new ObservableCollection<FeedItem>();

        public ICommand LoadCommand
        {
            get
            {
                return this.loadCommand ?? (this.loadCommand = new ActionCommand(() => this.StartLoading(), () => !this.running));
            }
        }

        public ICommand OpenCommand
        {
            get
            {
                return this.openCommand ?? (this.openCommand = new ActionExCommand<FeedItem>(o => this.Open(o)));
            }
        }

        #endregion

        #region Methods

        private void Load()
        {
            try
            {
                this.running = true;

                string[] urls = { "http://www.heise.de/newsticker/heise-atom.xml", "http://rss.golem.de/rss.php?feed=RSS2.0" };

                var feeds = new List<SyndicationFeed>();
                foreach (var url in urls)
                {
                    var reader = XmlReader.Create(url);
                    feeds.Add(SyndicationFeed.Load(reader));
                    reader.Close();
                }

                var newFeedItems = new List<FeedItem>();
                foreach (var feed in feeds)
                {
                    newFeedItems.AddRange(feed.Items.Select(item => new FeedItem(feed, item)).Except(this.FeedItems));
                }

                IList<FeedItem> newest = newFeedItems.OrderByDescending(item => item.Publish).Take(3).ToList();
                Application.Current.Dispatcher.Invoke(
                    () =>
                        {
                            newFeedItems.ForEach(this.FeedItems.Add);

                            if (newest.Any())
                            {
                                if (this.notificationWindow.IsVisible)
                                {
                                    this.notificationWindow.DisplayItems(newest);
                                    this.notificationWindow.Activate();
                                }
                                else
                                {
                                    this.notificationWindow = new NotificationWindow();
                                    this.notificationWindow.DisplayItems(newest);
                                    this.notificationWindow.Show();
                                }
                            }
                        });
            }
            finally
            {
                this.running = false;
                //this.OnPropertyChanged(() => this.LoadCommand);
                this.loadCommand.TriggerExecuteChange();
            }
        }

        private void Open(FeedItem feedItem)
        {
            Process.Start(feedItem.Item.Links.First().Uri.OriginalString);
        }

        private void StartLoading()
        {
            var task = new Task(
                () => { this.Load(); });
            task.Start();
        }

        #endregion
    }
}