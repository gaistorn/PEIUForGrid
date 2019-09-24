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
    public class ContractStatusCodesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            ContractStatusCodes status = (ContractStatusCodes)value;
            switch(status)
            {
                case ContractStatusCodes.Signing:
                    return "가입대기";
                case ContractStatusCodes.Cantacting:
                    return "상담대기";
                case ContractStatusCodes.Activating:
                    return "활성화";
                case ContractStatusCodes.Cancellations:
                    return "취소";
                case ContractStatusCodes.Deactivating:
                    return "비활성화";
                case ContractStatusCodes.Explorating:
                    return "출장대기";
                case ContractStatusCodes.WaitingForApprval:
                    return "내부협의대기";
                default:
                    return "Unknown";


            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
