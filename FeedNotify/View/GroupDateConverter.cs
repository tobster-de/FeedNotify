// GroupDateConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;

namespace FeedNotify
{
    public class GroupDateConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime)
            {
                var dt = (DateTime)value;
                if (dt.Date == DateTime.Now.Date)
                {
                    return "Heute";
                }

                if ((DateTime.Now.Date - dt.Date).Days == 1)
                {
                    return "Gestern";
                }

                if ((DateTime.Now.Date - dt.Date).Days < 5)
                {
                    return $"{dt.Date.DayOfWeek} ({dt.Date.ToShortDateString()})";
                }

                return dt.ToShortDateString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}