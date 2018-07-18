using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using HtmlTextBlock;
using System.IO;

namespace HtmlTextBlock
{
    public class HtmlHighlightTextBlock : TextBlock
    {
        public string Highlight
        {
            get => (string)this.GetValue(HtmlHighlightTextBlock.HighlightProperty);
            set => this.SetValue(HtmlHighlightTextBlock.HighlightProperty, value);
        }


        public static readonly DependencyProperty HighlightProperty =
        DependencyProperty.Register("Highlight", typeof(string), 
            typeof(HtmlHighlightTextBlock), new UIPropertyMetadata("Highlight", new PropertyChangedCallback(HtmlHighlightTextBlock.OnHighlightChanged)));


        public static DependencyProperty HtmlProperty = DependencyProperty.Register("Html", typeof(string),
            typeof(HtmlHighlightTextBlock), new UIPropertyMetadata("Html", new PropertyChangedCallback(HtmlHighlightTextBlock.OnHtmlChanged)));

        public string Html {
            get => (string)this.GetValue(HtmlHighlightTextBlock.HtmlProperty);
            set => this.SetValue(HtmlHighlightTextBlock.HtmlProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.Parse(this.Html);
        }

        private void Parse(string html, string highlight = null)
        {
            if (html == null)
            {
                return;
            }

            if (!String.IsNullOrEmpty(highlight) && html.Length >= highlight.Length && highlight.Length >= 3)
            {
                int idx = html.IndexOf(highlight, StringComparison.InvariantCultureIgnoreCase);
                //if (idx > 0)
                //{
                //    html = html.Replace(highlight, $"<b>{highlight}</b>", );
                //}

                while (idx != -1)
                { 
                    string span = "<font background=gold>";
                    html = String.Format("{0}{1}{2}</font>{3}",
                        html.Substring(0, idx), span, html.Substring(idx, this.Highlight.Length), html.Substring(idx + this.Highlight.Length));
                    idx = html.IndexOf(this.Highlight, idx + span.Length + this.Highlight.Length, StringComparison.InvariantCultureIgnoreCase);
                }
            }

            this.Inlines.Clear();
            HtmlTagTree tree = new HtmlTagTree();
            HtmlParser parser = new HtmlParser(tree); //output
            parser.Parse(new StringReader(html));     //input

            HtmlUpdater updater = new HtmlUpdater(this); //output
            updater.Update(tree);
        }

        public static void OnHtmlChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            HtmlHighlightTextBlock sender = (HtmlHighlightTextBlock)s;
            sender.Parse((string)e.NewValue, sender.Highlight);
        }

        public static void OnHighlightChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            HtmlHighlightTextBlock sender = (HtmlHighlightTextBlock)s;
            sender.Parse(sender.Html, (string)e.NewValue);
        }

        public HtmlHighlightTextBlock()
        {
            Text = "Assign Html Property";
        }

    }
}
