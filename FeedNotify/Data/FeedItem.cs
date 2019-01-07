// FeedItem.cs
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;

namespace FeedNotify.Data
{
    [DebuggerDisplay("{Feed}: {Id}")]
    public class FeedItem
    {
        #region Constructors and Destructors

        public FeedItem(SyndicationFeed feed, SyndicationItem item)
        {
            this.Item = item;
            this.Feed = WebUtility.HtmlDecode(feed.Title.Text);
            this.Title = WebUtility.HtmlDecode(item.Title.Text);
            this.Summary = WebUtility.HtmlDecode(item.Summary.Text);
            this.Publish = item.PublishDate.DateTime;
            this.Id = item.Id;
            this.Url = item.Links.FirstOrDefault()?.Uri.OriginalString ?? string.Empty;
        }

        #endregion

        #region Public Properties

        public string Feed { get; }

        public string Id { get; set; }

        public SyndicationItem Item { get; }

        public DateTime Publish { get; }

        public string Summary { get; }

        public string Title { get; }

        public string Url { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>
        ///     true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            var other = obj as FeedItem;
            if (other == null)
            {
                return false;
            }

            return this.Equals(other);
        }

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///     A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Id?.GetHashCode() ?? 0;
                //hashCode = (hashCode * 397) ^ this.Publish.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.Feed?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        #endregion

        #region Methods

        protected bool Equals(FeedItem other)
        {
            return string.Equals(this.Id, other.Id) /*&& this.Publish.Equals(other.Publish)*/&& string.Equals(this.Feed, other.Feed);
        }

        #endregion
    }
}