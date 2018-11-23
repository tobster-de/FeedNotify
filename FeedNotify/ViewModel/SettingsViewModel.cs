using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FeedNotify.Data;
using FeedNotify.Properties;
using FeedNotify.View;

namespace FeedNotify.ViewModel
{
    public class SettingsViewModel : NotifyPropertyChanged
    {
        private ICommand okCommand;
        private bool closeTrigger;
        private bool? result;

        public string FeedsText { get; set; }

        public ObservableCollection<string> Feeds { get; } = new ObservableCollection<string>();

        public int Interval { get; set; }

        public ICommand OkCommand
        {
            get
            {
                return this.okCommand ?? (this.okCommand = new ActionCommand(this.SaveAndClose, () => true));
            }
        }

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
        
        public bool? Result
        {
            get
            {
                return this.result;
            }
            set
            {
                this.SetValue(ref this.result, value);
            }
        }

        public int MaxAge { get; set; }

        public SettingsViewModel()
        {
            Settings settings = Properties.Settings.Default;
            this.Interval = settings.Interval;
            this.MaxAge = settings.MaxAge;

            if (settings.Feeds != null && settings.Feeds.Any())
            {
                this.FeedsText = string.Join("\n", settings.Feeds);
            }
        }

        private void SaveAndClose()
        {
            Properties.Settings.Default.Interval = this.Interval;
            Properties.Settings.Default.MaxAge = this.MaxAge;

            var feeds = new List<string>();
            this.FeedsText.Split('\n').ToList().ForEach(s => feeds.Add(s.Trim().Trim('\r', '\n')));
            Properties.Settings.Default.Feeds = new List<string>(feeds);
            Properties.Settings.Default.Save();

            this.Result = true;
            this.CloseTrigger = true;
        }
    }
}