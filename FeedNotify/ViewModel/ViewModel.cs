// ViewModel.cs
using System;
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
using FeedNotify.Control;
using FeedNotify.Data;
using FeedNotify.Properties;
using FeedNotify.View;

namespace FeedNotify.ViewModel
{
    internal class ViewModel : NotifyPropertyChanged
    {
        #region Fields

        private ActionCommand loadCommand;

        private NotificationWindow notificationWindow;

        private ICommand openCommand;
        private ICommand settingsCommand;

        private bool running;

        private Timer timer;

        private int reloadInterval = 60;

        private int timerInterval = 500;

        private double timeout;

        private double timeoutValue;

        private string filterText;

        private int maxFeedAge;

        #endregion

        #region Constructors and Destructors

        public ViewModel()
        {
            this.notificationWindow = new NotificationWindow();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            this.LoadSettings();
            //this.InitTimeout();
            this.StartLoading(false);
            this.timer = new Timer(o => this.TimingStep(), null, this.timerInterval, this.timerInterval);
        }

        public void LoadSettings()
        {
            Settings settings = Properties.Settings.Default;
            this.reloadInterval = settings.Interval;
            this.maxFeedAge = settings.MaxAge;

            if (settings.Feeds != null && settings.Feeds.Any())
            {
                this.Feeds = new List<string>(settings.Feeds);
            }
            else
            {
                this.Feeds = new List<string>();
            }
        }

        private void InitTimeout()
        {
            this.timeoutValue = this.timeout = this.reloadInterval * 1000;
        }

        #endregion

        #region Public Properties

        public ObservableCollection<FeedItem> FeedItems { get; } = new ObservableCollection<FeedItem>();

        public ICommand LoadCommand
        {
            get
            {
                return this.loadCommand ?? (this.loadCommand = new ActionCommand(() => this.StartLoading(false), () => !this.running));
            }
        }

        public ICommand SettingsCommand
        {
            get
            {
                return this.settingsCommand ?? (this.settingsCommand = new ActionCommand(() => this.OpenSettings()));
            }
        }

        public ICommand OpenCommand
        {
            get
            {
                return this.openCommand ?? (this.openCommand = new ActionExCommand<FeedItem>(o => this.Open(o)));
            }
        }

        public double TimeoutPercentage => this.timeoutValue * 100 / this.timeout;

        public List<string> Feeds { get; set; }

        public string FilterText
        {
            get => this.filterText;
            set
            {
                this.SetValue(ref this.filterText, value);

                this.SyncAnnotations();
            }
        }

        public ObservableCollection<AnnotatedListBox.Annotation> Annotations { get; } = new ObservableCollection<AnnotatedListBox.Annotation>();

        #endregion

        #region Methods

        private void Load(bool notify)
        {
            try
            {
                this.running = true;

                if (this.Feeds == null || !this.Feeds.Any())
                {
                    return;
                }

                //string[] urls = { "http://www.heise.de/newsticker/heise-atom.xml", "http://rss.golem.de/rss.php?feed=RSS2.0" };

                List<SyndicationFeed> feeds = new List<SyndicationFeed>();
                foreach (string url in this.Feeds)
                {
                    XmlReader reader = XmlReader.Create(url);
                    feeds.Add(SyndicationFeed.Load(reader));
                    reader.Close();
                }

                List<FeedItem> newFeedItems = new List<FeedItem>();
                foreach (SyndicationFeed feed in feeds)
                {
                    IEnumerable<SyndicationItem> youngEnough
                        = feed.Items.Where(i => (DateTime.Now - i.LastUpdatedTime).Days <= this.maxFeedAge
                                                || (DateTime.Now - i.PublishDate).Days <= this.maxFeedAge);
                    newFeedItems.AddRange(youngEnough.Select(item => new FeedItem(feed, item)).Except(this.FeedItems));
                }

                IList<FeedItem> newest = newFeedItems.OrderByDescending(item => item.Publish).Take(3).ToList();
                Application.Current.Dispatcher.Invoke(
                    () =>
                    {
                        newFeedItems.ForEach(this.FeedItems.Add);

                        if (notify && newest.Any())
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
                        
                        this.SyncAnnotations();
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

        private void TimingStep()
        {
            this.timeoutValue -= this.timerInterval;
            this.OnPropertyChanged(nameof(this.TimeoutPercentage));

            if (this.timeoutValue <= 0)
            {
                this.StartLoading();
            }
        }

        private void StartLoading(bool notify = true)
        {
            this.InitTimeout();
            Task task = new Task(() => { this.Load(notify); });
            task.Start();
        }

        private void OpenSettings()
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            if (settingsWindow.ShowDialog().GetValueOrDefault())
            {
                SettingsViewModel settingsViewModel = ((SettingsViewModel)settingsWindow.DataContext);
                this.Feeds = new List<string>(settingsViewModel.Feeds);

                this.reloadInterval = settingsViewModel.Interval;
                InitTimeout();
            }
        }

        public static bool StringContains(string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        private void SyncAnnotations()
        {
            List<FeedItem> toBeAnnotated =
                this.filterText.Length >= 3
                    ? this.FeedItems.Where(
                        f => StringContains(f.Title, this.filterText, StringComparison.InvariantCultureIgnoreCase)
                             || StringContains(f.Summary, this.filterText, StringComparison.InvariantCultureIgnoreCase)).ToList()
                    : new List<FeedItem>();

            IList<AnnotatedListBox.Annotation> toBeRemoved = this.Annotations.Where(a => !toBeAnnotated.Contains(a.SourceItem)).ToList();
            foreach (AnnotatedListBox.Annotation anno in toBeRemoved)
            {
                this.Annotations.Remove(anno);
            }

            IList<FeedItem> toBeAdded = toBeAnnotated.Except(this.Annotations.Select(a => a.SourceItem)).OfType<FeedItem>().ToList();
            foreach (FeedItem feedItem in toBeAdded)
            {
                this.Annotations.Add(new AnnotatedListBox.Annotation(feedItem, AnnotatedListBox.AnnotationTypeEnum.Search ));
            }
        }

        #endregion
    }
}