using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

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
        
        private StackPanel itemPanel;

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

        private Canvas AnnotationCanvas
        {
            get
            {
                if (this.annotationCanvas != null)
                {
                    return this.annotationCanvas;
                }

                ScrollBar scrollBar = AnnotatedListBox.GetVisualChild<ScrollBar>(this);
                this.annotationCanvas = (Canvas)scrollBar.Template.FindName("AnnotationCanvas", scrollBar);

                if (this.annotationCanvas != null)
                {
                    Rectangle background = (Rectangle)scrollBar.Template.FindName("AnnotationBackground", scrollBar);
                    background.MouseLeftButtonDown += this.BackgroundOnMouseLeftButtonDown;
                }

                return this.annotationCanvas;
            }
        }

        private void BackgroundOnMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            if (!(sender is FrameworkElement fe))
            {
                return;
            }

            ScrollBar scrollBar = AnnotatedListBox.GetVisualChild<ScrollBar>(this);
            double value = scrollBar.Track.ValueFromPoint(args.GetPosition(fe));

            if (value < scrollBar.Track.Value)
            {
                ScrollBar.PageUpCommand.Execute(null, scrollBar);
            }
            else
            {
                ScrollBar.PageDownCommand.Execute(null, scrollBar);
            }
        }

        private Panel ItemPanel
        {
            get
            {
                if (this.itemPanel != null)
                {
                    return this.itemPanel;
                }

                this.itemPanel = AnnotatedListBox.GetVisualChild<StackPanel>(this);
                return this.itemPanel;
            }
        }

        public AnnotatedListBox()
        {
            this.SizeChanged += this.OnSizeChanged;
        }

        /// <inheritdoc />
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (!this.IsLoaded || !this.Annotations.Any())
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                this.Dispatcher.InvokeAsync(() => { this.UpdateAnnotations(); }, DispatcherPriority.Background);
            }
        }

        public void UpdateAnnotations()
        {
            if (!this.IsLoaded || !this.Annotations.Any())
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
            if (this.AnnotationCanvas == null)
            {
                return;
            }

            foreach (Annotation o in annotations)
            {
                if (this.annotationDictionary.TryGetValue(o, out FrameworkElement fe))
                {
                    fe.MouseLeftButtonDown -= this.ClickAnnotation;

                    this.annotationCanvas.Children.Remove(fe);
                    this.annotationDictionary.Remove(o);
                }
            }
        }

        private void AddAnnotations(IList<Annotation> newAnnotations)
        {
            if (this.AnnotationCanvas == null || this.ItemPanel == null)
            {
                return;
            }

            double panelHeight = this.ItemPanel.ActualHeight;
            double canvasHeight = this.AnnotationCanvas.ActualHeight;
            double width = this.AnnotationCanvas.ActualWidth;
            bool hasAllHeights = true;

            foreach (Annotation o in newAnnotations)
            {
                if (this.annotationDictionary.ContainsKey(o))
                {
                    continue;
                }

                SolidColorBrush brush = o.Type == AnnotationTypeEnum.Selection ? Brushes.DodgerBlue : Brushes.Orange;

                hasAllHeights &= this.GetItemLocation(o.SourceItem, out double top, out double itemHeight);

                Shape shape = null;
                if (this.AnnotationStyle == AnnotationStyleEnum.Line)
                {
                    double avg = itemHeight / 2 + top;
                    double p = (canvasHeight * avg / panelHeight);
                    shape = new Line() { X1 = 0, Y1 = p, X2 = width, Y2 = p, StrokeThickness = 2, Stroke = brush };
                }
                else if (this.AnnotationStyle == AnnotationStyleEnum.Region)
                {
                    double h = (canvasHeight * itemHeight / panelHeight);
                    shape = new Rectangle() { Width = width, Height = (h > 2 ? h : 2), Fill = brush };
                }

                if (shape == null)
                {
                    continue;
                }

                //shape.Visibility = hasAllHeights ? Visibility.Visible : Visibility.Hidden;

                this.annotationDictionary.Add(o, shape);

                this.AnnotationCanvas.Children.Add(shape);
                if (this.AnnotationStyle == AnnotationStyleEnum.Region)
                {
                    double p = (canvasHeight * top / panelHeight);
                    Canvas.SetTop(shape, p);
                }

                if (o.Type == AnnotationTypeEnum.Search)
                {
                    shape.Cursor = Cursors.Hand;
                    shape.MouseLeftButtonDown += this.ClickAnnotation;
                }
            }

            if (!hasAllHeights)
            {
                this.Dispatcher.InvokeAsync(() => { this.UpdateAnnotations(); }, DispatcherPriority.Background);
            }
        }

        private void ClickAnnotation(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is FrameworkElement fe))
            {
                return;
            }

            KeyValuePair<Annotation, FrameworkElement> entry = this.annotationDictionary.FirstOrDefault(kv => object.Equals(kv.Value, fe));
            object item = entry.Key.SourceItem;

            if (item != null)
            {
                this.SelectedItem = item;
                this.GetItemLocation(item, out double top, out double itemheight);
                ScrollViewer scrollViewer = AnnotatedListBox.GetVisualChild<ScrollViewer>(this);
                scrollViewer?.ScrollToVerticalOffset(top - scrollViewer.ViewportHeight / 2 + itemheight / 2);
            }
        }

        private void MoveAnnotations(List<Annotation> annotations)
        {
            if (this.AnnotationCanvas == null || this.ItemPanel == null || !annotations.Any())
            {
                return;
            }

            double panelHeigth = this.ItemPanel.ActualHeight;
            double canvasHeight = this.AnnotationCanvas.ActualHeight;
            bool hasAllHeights = true;

            foreach (Annotation o in annotations)
            {
                if (!this.annotationDictionary.TryGetValue(o, out FrameworkElement fe)
                    || !(fe is Shape))
                {
                    continue;
                }

                //fe.Visibility = hasAllHeights ? Visibility.Visible : Visibility.Hidden;

                hasAllHeights &= this.GetItemLocation(o.SourceItem, out double top, out double itemheight);

                if (this.AnnotationStyle == AnnotationStyleEnum.Line && fe is Line line)
                {
                    double avg = itemheight / 2 + top;
                    double p = (canvasHeight * avg / panelHeigth);

                    line.Y1 = p;
                    line.Y2 = p;
                }
                else if (this.AnnotationStyle == AnnotationStyleEnum.Region && fe is Rectangle rect)
                {
                    double h = (canvasHeight * itemheight / panelHeigth);
                    rect.Height = h > 2 ? h : 2;
                    double p = (canvasHeight * top / panelHeigth);
                    Canvas.SetTop(rect, p);
                }
            }

            if (!hasAllHeights)
            {
                this.Dispatcher.InvokeAsync(() => { this.UpdateAnnotations(); }, DispatcherPriority.Background);
            }
        }

        private bool GetItemLocation(object item, out double top, out double height)
        {
            top = 0;
            height = 1;

            if (this.ItemPanel == null)
            {
                return false;
            }

            DependencyObject container = this.ItemContainerGenerator.ContainerFromItem(item);
            if (container == null)
            {
                return false;
            }

            if (container is FrameworkElement fe)
            {
                GeneralTransform positionTransform = fe.TransformToAncestor(this.ItemPanel);
                Point location = positionTransform.Transform(new Point(0, 0));

                top = location.Y;
                height = fe.ActualHeight;
            }

            return true;
        }
    }
}
