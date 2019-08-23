using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEIU.GUI.ViewModel
{
    public class ViewModelLocator
    {
        private static MainWindowViewModel _mainStatic;
        private static CustomerManagerViewModel _customersStatic;

        public static MainWindowViewModel MainStatic { get { return _mainStatic ?? (_mainStatic = new MainWindowViewModel()); } }
        public static CustomerManagerViewModel CustomersStatic { get { return _customersStatic ?? (_customersStatic = new CustomerManagerViewModel()); } }

        static ViewModelLocator()
        {
        }

        public MainWindowViewModel Main => MainStatic;
        public CustomerManagerViewModel Customers => CustomersStatic;
        

        public ViewModelLocator()
        {
        }

        public static void Dispose()
        {
            
        }

    }
}
