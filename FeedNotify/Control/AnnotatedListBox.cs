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
        public enum AnnotationStyleEnum
        {
            Line,
            Region
        }

        public enum AnnotationTypeEnum
        {
            Selection,
            Search
        }

        public struct Annotation
        {
            public object SourceItem;

            public AnnotationTypeEnum Type;

            public Annotation(object sourceItem, AnnotationTypeEnum type)
            {
                this.SourceItem = sourceItem;
                this.Type = type;
            }
        }

        private Canvas annotationCanvas;

        private Dictionary<Annotation, FrameworkElement> annotationDictionary = new Dictionary<Annotation, FrameworkElement>();

        public static readonly DependencyProperty AnnotationsProperty =
            DependencyProperty.Register(
                nameof(AnnotatedListBox.Annotations),
                typeof(ObservableCollection<Annotation>),
                typeof(AnnotatedListBox),
                new UIPropertyMetadata(AnnotatedListBox.AnnotationsPropertyChanged));

        public static readonly DependencyProperty AnnotationStyleProperty =
            DependencyProperty.Register(
                nameof(AnnotatedListBox.AnnotationStyle),
                typeof(AnnotationStyleEnum),
                typeof(AnnotatedListBox),
                new UIPropertyMetadata(AnnotationStyleEnum.Line));

        private static void AnnotationsPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (!(target is AnnotatedListBox control))
            {
                return;
            }

            control.Annotations = e.NewValue as ObservableCollection<AnnotatedListBox.Annotation>;
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

        public AnnotationStyleEnum AnnotationStyle
        {
            get => (AnnotationStyleEnum)this.GetValue(AnnotatedListBox.AnnotationStyleProperty);
            set => this.SetValue(AnnotatedListBox.AnnotationStyleProperty, value);
        }

        public AnnotatedListBox()
        {
            this.Loaded += this.OnLoaded;
            this.SizeChanged += this.OnSizeChanged;
            this.ItemContainerGenerator.ItemsChanged += (sender, args) => this.OnItemsChanged(args);
        }

        private void OnItemsChanged(ItemsChangedEventArgs e)
        {
            if (!this.IsLoaded)
            {
                return;
            }

            IList<Annotation> obsolete = this.Annotations.Where(o => !this.Items.Contains(o.SourceItem)).ToList();
            if (obsolete.Any())
            {
                this.RemoveAnnotations(obsolete);
            }

            this.MoveAnnotations(this.annotationDictionary.Keys.ToList());
        }

        /// <inheritdoc />
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            IList<Annotation> toBeRemoved = this.annotationDictionary.Keys
                                                .Where(a => a.Type == AnnotationTypeEnum.Selection && e.RemovedItems.Contains(a.SourceItem)).ToList();
            if (toBeRemoved.Any())
            {
                this.RemoveAnnotations(toBeRemoved);
            }

            IList<Annotation> toBeAdded = e.AddedItems.OfType<object>().Select(o => new Annotation(o, AnnotationTypeEnum.Selection)).ToList();
            if (toBeAdded.Any())
            {
                this.AddAnnotations(toBeAdded);
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.HeightChanged)
            {
                return;
            }

            this.MoveAnnotations(this.annotationDictionary.Keys.ToList());
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

                SolidColorBrush brush = o.Type == AnnotationTypeEnum.Selection ? Brushes.DodgerBlue : Brushes.Orange;

                this.CalcPosition(o.SourceItem, itemHeights, out double begin, out double end);

                Shape shape = null;
                if (this.AnnotationStyle == AnnotationStyleEnum.Line)
                {
                    double avg = (end - begin) / 2 + begin;
                    double p = (height * avg / sumHeight);
                    shape = new Line() { X1 = 0, Y1 = p, X2 = width, Y2 = p, StrokeThickness = 2, Stroke = brush };
                }
                else if (this.AnnotationStyle == AnnotationStyleEnum.Region)
                {
                    double size = (end - begin);
                    double h = (height * size / sumHeight);
                    shape = new Rectangle() { Width = width, Height = (h > 2 ? h : 2), Fill = brush };
                }

                if (shape == null)
                {
                    continue;
                }

                this.annotationDictionary.Add(o, shape);

                this.annotationCanvas.Children.Add(shape);
                if (this.AnnotationStyle == AnnotationStyleEnum.Region)
                {
                    double p = (height * begin / sumHeight);
                    Canvas.SetTop(shape, p);
                }
            }
        }

        private void MoveAnnotations(List<Annotation> annotations)
        {
            if (!this.CheckAnnotationCanvas() || !annotations.Any())
            {
                return;
            }

            double height = this.annotationCanvas.ActualHeight;
            Dictionary<object, double> itemHeights = this.GetItemHeights();
            double sumHeight = itemHeights.Values.Sum();

            foreach (Annotation o in annotations)
            {
                if (!this.annotationDictionary.TryGetValue(o, out FrameworkElement fe)
                    || !(fe is Shape))
                {
                    continue;
                }

                this.CalcPosition(o.SourceItem, itemHeights, out double begin, out double end);
                if (this.AnnotationStyle == AnnotationStyleEnum.Line && fe is Line line)
                {
                    double avg = (end - begin) / 2 + begin;
                    double p = (height * avg / sumHeight);

                    line.Y1 = p;
                    line.Y2 = p;
                }
                else if (this.AnnotationStyle == AnnotationStyleEnum.Region && fe is Rectangle rect)
                {
                    double size = (end - begin);
                    double h = (height * size / sumHeight);
                    rect.Height = h > 2 ? h : 2;
                    double p = (height * begin / sumHeight);
                    Canvas.SetTop(rect, p);
                }
            }
        }

        private void CalcPosition(
            object item,
            Dictionary<object, double> itemHeights,
            out double begin,
            out double end)
        {
            begin = -1;
            end = -1;

            int index = this.Items.IndexOf(item);
            if (index < 0)
            {
                return;
            }

            if (index >= 0)
            {
                begin = itemHeights.Values.ToList().Take(index).Sum();
                end = begin + itemHeights[item];
            }
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
