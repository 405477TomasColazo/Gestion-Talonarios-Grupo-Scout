using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GestionTalonarios.UI.Converters
{
    public class MultiValueBooleanToVisibilityConverter: IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Si algún valor es null o no es bool, retorna Collapsed
            if (values == null)
                return Visibility.Collapsed;

            // Para el caso específico, verificamos que:
            // 1. Hay al menos un valor
            // 2. El primer valor (IsDelivered) es true
            if (values.Length > 0 && values[0] is bool isDelivered && isDelivered)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
