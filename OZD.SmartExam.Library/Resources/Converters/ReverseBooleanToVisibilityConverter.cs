namespace OZD.SmartExam.Library.Resources.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Revers boolean converter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ReverseBooleanConverter<T> : IValueConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReverseBooleanConverter{T}"/> class.
        /// </summary>
        /// <param name="trueValue">The true value.</param>
        /// <param name="falseValue">The false value.</param>
        public ReverseBooleanConverter(T trueValue, T falseValue)
        {
            True = trueValue;
            False = falseValue;
        }

        /// <summary>
        /// Gets or sets the true.
        /// </summary>
        /// <value>
        /// The true.
        /// </value>
        public T True { get; set; }

        /// <summary>
        /// Gets or sets the false.
        /// </summary>
        /// <value>
        /// The false.
        /// </value>
        public T False { get; set; }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null" />, the valid null value is used.
        /// </returns>
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? False : True;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null" />, the valid null value is used.
        /// </returns>
        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, False);
        }
    }

    /// <summary>
    /// Converter to reverse the visibility.
    /// </summary>
    /// <seealso cref="OZD.SmartExam.Library.Converters.ReverseBooleanConverter{System.Windows.Visibility}" />
    public sealed class ReverseBooleanToVisibilityConverter : ReverseBooleanConverter<Visibility>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReverseBooleanToVisibilityConverter"/> class.
        /// </summary>
        public ReverseBooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        {
        }
    }
}