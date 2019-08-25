using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEIU.GUI.ViewModel
{
    public class ViewModelLocator
    {
        public static MainWindowViewModel MainStatic => SimpleIoc.Default.GetInstance<MainWindowViewModel>();
        public static CustomerManagerViewModel CustomersStatic => SimpleIoc.Default.GetInstance<CustomerManagerViewModel>();



        static ViewModelLocator()
        {
        }

        public MainWindowViewModel Main => MainStatic;
        public CustomerManagerViewModel Customers => CustomersStatic;

        public StatusDashboardViewModel StatusDashboard => SimpleIoc.Default.GetInstance<StatusDashboardViewModel>();

       

        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            //SimpleIoc.Default.Register<Models.IBaseModel, MainWindowViewModel>();
            SimpleIoc.Default.Register<MainWindowViewModel>();
            SimpleIoc.Default.Register< CustomerManagerViewModel>();
            //SimpleIoc.Default.Register<StatusDashboardViewModel>();
            SimpleIoc.Default.Register<StatusDashboardViewModel>();
        }

        public static void Dispose()
        {
            
        }

    }
}
