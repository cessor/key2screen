using System;
using System.Globalization;
using System.Windows.Data;

namespace Key2Screen
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class IsNullToBooleanConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return value == null;
        }

        public object ConvertBack(object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}