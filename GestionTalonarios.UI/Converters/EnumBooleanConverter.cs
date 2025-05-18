using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GestionTalonarios.UI.Converters
{
    public class EnumBooleanConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null)
                return false;

            string parameterString = parameter.ToString();
            if (Enum.IsDefined(value.GetType(), value))
            {
                string valueString = Enum.GetName(value.GetType(), value);
                return string.Equals(valueString, parameterString);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null || !(bool)value)
                return Binding.DoNothing;

            string parameterString = parameter.ToString();
            if (Enum.IsDefined(targetType, parameterString))
            {
                return Enum.Parse(targetType, parameterString);
            }

            return Binding.DoNothing;
        }
    }
}

