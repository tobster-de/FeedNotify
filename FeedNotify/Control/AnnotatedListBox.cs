using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
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

        private Dictionary<Annotation, FrameworkElement> annotationDictionary = new Dictionary<Annotation, FrameworkElement>();

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

        public ObservableCollection<Annotation> Annotations
        {
            get => (ObservableCollection<Annotation>)this.GetValue(AnnotatedListBox.AnnotationsProperty);
            set
            {
                ObservableCollection<Annotation> oldValue = (ObservableCollection<Annotation>)this.GetValue(AnnotatedListBox.AnnotationsProperty);
                if (oldValue != null)
                {
                    oldValue.CollectionChanged -= this.AnnotationCollectionChanged;
                }

                this.SetValue(AnnotatedListBox.AnnotationsProperty, value);

                if (value != null)
                {
                    value.CollectionChanged += this.AnnotationCollectionChanged;
                }
            }
        }

        public AnnotatedListBox()
        {
            this.Loaded += this.OnLoaded;
            this.SizeChanged += this.OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.HeightChanged)
            {
                return;
            }

            this.MoveAnnotations(this.Annotations.ToList());
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.CheckAnnotationCanvas();
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

        private bool CheckAnnotationCanvas()
        {
            if (this.annotationCanvas != null)
            {
                return true;
            }

            ScrollBar scrollBar = AnnotatedListBox.GetVisualChild<ScrollBar>(this);
            this.annotationCanvas = (Canvas)scrollBar.Template.FindName("AnnotationCanvas", scrollBar);

            return this.annotationCanvas != null;
        }

        // Fill the Canvas with horizontal markers. Can be optimized.
        private void AnnotationCollectionChanged(object sender, NotifyCollectionChangedEventArgs cce)
        {
            switch (cce.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    this.AddAnnotations(cce.NewItems.OfType<Annotation>().ToList());
                    break;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    this.RemoveAnnotations(cce.OldItems.OfType<Annotation>().ToList());
                    break;
                case NotifyCollectionChangedAction.Move:
                    this.MoveAnnotations(cce.OldItems.OfType<Annotation>().ToList());
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RemoveAnnotations(IList<Annotation> annotations)
        {
            if (!this.CheckAnnotationCanvas())
            {
                return;
            }

            foreach (Annotation o in annotations)
            {
                if (this.annotationDictionary.TryGetValue(o, out FrameworkElement fe))
                {
                    this.annotationCanvas.Children.Remove(fe);
                    this.annotationDictionary.Remove(o);
                }
            }
        }

        private void AddAnnotations(IList<Annotation> newAnnotations)
        {
            if (!this.CheckAnnotationCanvas())
            {
                return;
            }

            double height = this.annotationCanvas.ActualHeight;
            double width = this.annotationCanvas.ActualWidth;
            Dictionary<object, double> itemHeights = this.GetItemHeights();
            double sumHeight = itemHeights.Values.Sum();

            foreach (Annotation o in newAnnotations)
            {
                if (this.annotationDictionary.ContainsKey(o))
                {
                    continue;
                }

                int p = this.CalcPosition(o.SourceItem, itemHeights, height, sumHeight);

                Line line = new Line() { X1 = 0, Y1 = p, X2 = width, Y2 = p, StrokeThickness = 2, Stroke = Brushes.Orange };
                this.annotationCanvas.Children.Add(line);

                this.annotationDictionary.Add(o, line);
            }
        }

        private void MoveAnnotations(List<Annotation> annotations)
        {
            if (!this.CheckAnnotationCanvas())
            {
                return;
            }

            double height = this.annotationCanvas.ActualHeight;
            Dictionary<object, double> itemHeights = this.GetItemHeights();
            double sumHeight = itemHeights.Values.Sum();

            foreach (Annotation o in annotations)
            {
                if (!this.annotationDictionary.TryGetValue(o, out FrameworkElement fe)
                    || !(fe is Line line))
                {
                    continue;
                }

                int p = this.CalcPosition(o.SourceItem, itemHeights, height, sumHeight);

                line.Y1 = p;
                line.Y2 = p;
            }
        }

        private int CalcPosition(object item, Dictionary<object, double> itemHeights, double height, double sumHeight)
        {
            int index = this.Items.IndexOf(item);
            if (index < 0)
            {
                return -1;
            }

            double calcedPos = itemHeights[item] / 2;
            if (index > 0)
            {
                calcedPos += itemHeights.Values.ToList().GetRange(0, index).Sum();
            }

            int p = (int)(height * calcedPos / sumHeight);
            return p;
        }

        private Dictionary<object, double> GetItemHeights()
        {
            Dictionary<object, double> itemHeights = new Dictionary<object, double>();
            foreach (object item in this.Items)
            {
                DependencyObject container = this.ItemContainerGenerator.ContainerFromItem(item);

                if (container is FrameworkElement fe)
                {
                    itemHeights.Add(item, fe.ActualHeight);
                }
            }

            return itemHeights;
        }
    }
}
