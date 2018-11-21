using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FeedNotify.Control
{
    public sealed class AnnotatedListBox : ListBox
    {
        public struct Annotation
        {
            public object SourceItem;
        }

        private Canvas annotationCanvas;

        public static readonly DependencyProperty AnnotationsProperty =
            DependencyProperty.Register(
                nameof(AnnotatedListBox.Annotations),
                typeof(ObservableCollection<Annotation>),
                typeof(AnnotatedListBox),
                new UIPropertyMetadata(AnnotatedListBox.AnnotationsPropertyChanged));

        private static void AnnotationsPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (!(target is AnnotatedListBox control))
            {
                return;
            }

            control.Annotations = e.NewValue as ObservableCollection<AnnotatedListBox.Annotation>;

            //if (e.NewValue != null && e.OldValue == null)
            //{
            //    ((ObservableCollection<object>)e.NewValue).CollectionChanged += AnnotatedListView.OnCollectionChanged;
            //}
            //else if (e.NewValue == null && e.OldValue != null)
            //{
            //    ((ObservableCollection<object>)e.OldValue).CollectionChanged -= AnnotatedListView.OnCollectionChanged;
            //}
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs cce)
        {
            this.UpdateAnnotations();
        }

        public ObservableCollection<Annotation> Annotations
        {
            get => (ObservableCollection<Annotation>)this.GetValue(AnnotatedListBox.AnnotationsProperty);
            set
            {
                ObservableCollection<Annotation> oldValue = (ObservableCollection<Annotation>)this.GetValue(AnnotatedListBox.AnnotationsProperty);
                if (oldValue != null)
                {
                    oldValue.CollectionChanged -= this.OnCollectionChanged;
                }

                this.SetValue(AnnotatedListBox.AnnotationsProperty, value);

                if (value != null)
                {
                    value.CollectionChanged += this.OnCollectionChanged;
                }
            }
        }

        public AnnotatedListBox()
        {
            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ScrollBar scrollBar = AnnotatedListBox.GetVisualChild<ScrollBar>(this);
            this.annotationCanvas = (Canvas)scrollBar.Template.FindName("AnnotationCanvas", this);

            //this.UpdateAnnotations();
        }

        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T ?? AnnotatedListBox.GetVisualChild<T>(v);
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        // Fill the Canvas with horizontal markers. Can be optimized.
        private void UpdateAnnotations()
        {
            if (this.annotationCanvas == null)
            {
                ScrollBar scrollBar = AnnotatedListBox.GetVisualChild<ScrollBar>(this);
                this.annotationCanvas = (Canvas)scrollBar.Template.FindName("AnnotationCanvas", scrollBar);

                if (this.annotationCanvas == null) return;
            }

            this.annotationCanvas.Children.Clear();

            double m = this.Items.Count;
            double height = this.ActualHeight;
            double width = this.annotationCanvas.ActualWidth;

            foreach (Annotation o in this.Annotations)
            {
                int i = this.Items.IndexOf(o.SourceItem);

                if (i < 0)
                {
                    continue;
                }

                int p = (int)(height * i / m);
                this.annotationCanvas.Children.Add(new Line() { X1 = 0, Y1 = p, X2 = width, Y2 = p, StrokeThickness = 2, Stroke = Brushes.Orange });
            }
        }

    }
}
