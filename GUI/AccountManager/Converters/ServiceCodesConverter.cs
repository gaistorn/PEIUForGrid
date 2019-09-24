using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PEIU.GUI.Converters
{
    public class ServiceCodesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ServiceCodes code = (ServiceCodes)value;
            switch(code)
            {
                case ServiceCodes.DR:
                    return "DR 서비스";
                case ServiceCodes.Schedule:
                    return "스케쥴링";
                default:
                    return "[미지정]";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
