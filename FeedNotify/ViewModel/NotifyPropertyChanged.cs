// NotifyPropertyChanged.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FeedNotify
{
    /// <summary>
    ///     Class providing basic implementation for view model classes which require to
    ///     notify a property change to the view.
    /// </summary>
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        #region Public Events

        /// <summary>
        ///     The property changed event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The basic method to notify property change. Property name will be predetermined by compiler.
        ///     Provide string.Empty as property name to trigger property change for all properties.
        /// </summary>
        /// <param name="propertyName">
        ///     The property name.
        /// </param>
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // this.VerifyPropertyName(propertyName);
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        ///     The basic method to notify property change. Provide the property using a lambda statement.
        /// </summary>
        /// <example>
        ///     OnPropertyChanged(() => this.Property);.
        /// </example>
        /// <typeparam name="T">
        ///     The type parameter.
        /// </typeparam>
        /// <param name="propertyExpression">
        ///     The property expresion.
        /// </param>
        public void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var body = (MemberExpression)propertyExpression.Body;
            if (body == null)
            {
                throw new ArgumentException("propertyExpression must return a property.");
            }

            this.VerifyPropertyExpression(propertyExpression, body);
            this.OnPropertyChanged(body.Member.Name);
        }

        /// <summary>
        ///     This Method fires the PropertyChanges of an Entity to the ViewModel Properties.
        /// </summary>
        /// <param name="sender">
        ///     Sender Entity.
        /// </param>
        /// <param name="e">
        ///     Property Change Event.
        /// </param>
        public virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e.PropertyName);
        }

        /// <summary>
        ///     Sets the value of the property backing field and triggers the property changed event for the property.
        ///     Property name will be predetermined by compiler.
        /// </summary>
        /// <typeparam name="T">
        ///     The type parameter.
        /// </typeparam>
        /// <param name="refValue">
        ///     The ref value.
        /// </param>
        /// <param name="newValue">
        ///     The new value.
        /// </param>
        /// <param name="propertyName">
        ///     The property name.
        /// </param>
        /// <example>
        ///     SetValue(ref this.backingfield, value);
        ///     .
        /// </example>
        public void SetValue<T>(ref T refValue, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(refValue, newValue))
            {
                return;
            }

            refValue = newValue;
            this.OnPropertyChanged(propertyName);
        }

        /// <summary>
        ///     Sets the value of a property backing field and triggers the property changed event for the property.
        ///     Use this method to change a property outside the corresponding property setter.
        /// </summary>
        /// <typeparam name="T">
        ///     The type parameter.
        /// </typeparam>
        /// <param name="refValue">
        ///     The ref value.
        /// </param>
        /// <param name="newValue">
        ///     The new value.
        /// </param>
        /// <param name="propertyExpression">
        ///     The property expresion.
        /// </param>
        /// <example>
        ///     SetValue(ref this.propertybackingfield, somevalue, () => this.Property);
        ///     .
        /// </example>
        public void SetValue<T>(ref T refValue, T newValue, Expression<Func<T>> propertyExpression)
        {
            if (EqualityComparer<T>.Default.Equals(refValue, newValue))
            {
                return;
            }

            refValue = newValue;
            this.OnPropertyChanged(propertyExpression);
        }

        /// <summary>
        ///     Sets the value.
        /// </summary>
        /// <typeparam name="T">
        ///     The type parameter.
        /// </typeparam>
        /// <param name="refValue">
        ///     The ref value.
        /// </param>
        /// <param name="newValue">
        ///     The new value.
        /// </param>
        /// <param name="valueChanged">
        ///     The value changed.
        /// </param>
        public void SetValue<T>(ref T refValue, T newValue, Action valueChanged)
        {
            if (EqualityComparer<T>.Default.Equals(refValue, newValue))
            {
                return;
            }

            refValue = newValue;
            valueChanged();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Call this method to clear the property changed event i.e. forcing handlers to disconnect.
        ///     Use this method when disposing instances of derived classes.
        /// </summary>
        protected void ClearPropertyChangedEvent()
        {
            this.PropertyChanged = null;
        }

        /// <summary>
        ///     Warns the developer if this object does not have a public property with the specified name.
        ///     This method does not exist in a Release build.
        /// </summary>
        /// <typeparam name="T">
        ///     The type parameter.
        /// </typeparam>
        /// <param name="propertyExpression">
        ///     The property expression.
        /// </param>
        /// <param name="property">
        ///     The property.
        /// </param>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        protected void VerifyPropertyExpression<T>(Expression<Func<T>> propertyExpression, MemberExpression property)
        {
            if (property.Member.GetType().IsAssignableFrom(typeof(PropertyInfo)))
            {
                Debug.Fail(string.Format(CultureInfo.CurrentCulture, "Invalid Property Expression {0}", propertyExpression));
            }
        }

        /// <summary>
        ///     Warns the developer if this object does not have a public property with the specified name.
        ///     This method does not exist in a Release build.
        /// </summary>
        /// <param name="propertyName">
        ///     The property Name.
        /// </param>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        private void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (!string.IsNullOrEmpty(propertyName) && TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                var msg = "Check OnPropertyChanged: There is no property \"" + propertyName + "\" in type " + this.GetType().FullName;
                Debug.Fail(msg);
            }
        }

        #endregion
    }
}