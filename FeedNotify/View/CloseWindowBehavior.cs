// CloseWindowBehavior.cs
using System.Windows;
using System.Windows.Interactivity;

namespace FeedNotify.View
{
    public class CloseWindowBehavior : Behavior<Window>
    {
        #region Static Fields

        public static readonly DependencyProperty CloseTriggerProperty =
            DependencyProperty.Register("CloseTrigger", typeof(bool), typeof(CloseWindowBehavior), new PropertyMetadata(false, OnCloseTriggerChanged));

        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.Register("DialogResult", typeof(bool?), typeof(CloseWindowBehavior), new PropertyMetadata(null));

        #endregion

        #region Public Properties

        public bool CloseTrigger
        {
            get
            {
                return (bool)this.GetValue(CloseTriggerProperty);
            }
            set
            {
                this.SetValue(CloseTriggerProperty, value);
            }
        }

        public bool? DialogResult
        {
            get
            {
                return (bool?)this.GetValue(DialogResultProperty);
            }
            set
            {
                this.SetValue(DialogResultProperty, value);
            }
        }

        #endregion

        #region Methods

        private static void OnCloseTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as CloseWindowBehavior;

            if (behavior != null)
            {
                behavior.OnCloseTriggerChanged();
            }
        }

        private void OnCloseTriggerChanged()
        {
            // when closetrigger is true, close the window
            if (this.CloseTrigger)
            {
                this.AssociatedObject.DialogResult = DialogResult;
                this.AssociatedObject.Close();
            }
        }

        #endregion
    }
}