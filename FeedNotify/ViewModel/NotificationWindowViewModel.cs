// NotificationWindowViewModel.cs
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using FeedNotify.Data;

namespace FeedNotify.ViewModel
{
    public class NotificationWindowViewModel : NotifyPropertyChanged
    {
        #region Fields

        private bool closeTrigger;

        private ICommand openCommand;

        private ICommand timeoutCommand;

        #endregion

        #region Public Properties

        public bool CloseTrigger
        {
            get
            {
                return this.closeTrigger;
            }
            set
            {
                this.SetValue(ref this.closeTrigger, value);
            }
        }

        public ObservableCollection<FeedItem> FeedItems { get; } = new ObservableCollection<FeedItem>();

        public ICommand OpenCommand
        {
            get
            {
                return this.openCommand ?? (this.openCommand = new ActionExCommand<FeedItem>(o => this.Open(o)));
            }
        }

        public ICommand TimeoutCommand
        {
            get
            {
                return this.timeoutCommand ?? (this.timeoutCommand = new ActionExCommand<FeedItem>(o => this.RemoveItem(o)));
            }
        }

        #endregion

        #region Public Methods and Operators

        public void SetDisplayItems(IEnumerable<FeedItem> displayItems)
        {
            foreach (var displayItem in displayItems)
            {
                this.FeedItems.Add(displayItem);
            }
        }

        #endregion

        #region Methods

        private void Open(FeedItem feedItem)
        {
            this.RemoveItem(feedItem);
            Process.Start(feedItem.Item.Links.First().Uri.OriginalString);
        }

        private void RemoveItem(FeedItem feedItem)
        {
            this.FeedItems.Remove(feedItem);
            if (!this.FeedItems.Any())
            {
                this.CloseTrigger = true;
            }
        }

        #endregion
    }
}