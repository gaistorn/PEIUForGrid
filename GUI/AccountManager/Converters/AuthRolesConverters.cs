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
    public class AuthRolesIconConverters : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //AuthRoles roles = (AuthRoles)value;
            //switch(roles)
            //{
            //    case AuthRoles.Aggregator:
            //        return MahApps.Metro.IconPacks.PackIconMaterialKind.AccountGroup;
            //    case AuthRoles.Business:
            //        return MahApps.Metro.IconPacks.PackIconMaterialKind.AccountTie;
            //    case AuthRoles.Candidator:
            //        return MahApps.Metro.IconPacks.PackIconMaterialKind.AccountClock;
            //    default:
            //        return MahApps.Metro.IconPacks.PackIconMaterialKind.Account;

            //}
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AuthRolesTitleConverters : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //IGrouping<AuthRoles, RegisterViewModel> registerViewModel = (IGrouping<AuthRoles, RegisterViewModel>)value;
            //AuthRoles roles = registerViewModel.Key;
            //switch (roles)
            //{
            //    case AuthRoles.Aggregator:
            //        return "중계거래자";
            //    case AuthRoles.Business:
            //        return "발전사업자";
            //    case AuthRoles.Candidator:
            //        return "가입대기자";
            //    default:
            //        return "운영자";

            //}
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
