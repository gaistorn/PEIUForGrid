using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using PEIU.Models;
using PEIU.Models.ExchangeModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

       public CandidateCustomerViewModel CandidateCustomer => SimpleIoc.Default.GetInstance<CandidateCustomerViewModel>();

        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            //SimpleIoc.Default.Register<Models.IBaseModel, MainWindowViewModel>();
            SimpleIoc.Default.Register<MainWindowViewModel>();
            SimpleIoc.Default.Register< CustomerManagerViewModel>();
            //SimpleIoc.Default.Register<StatusDashboardViewModel>();
            SimpleIoc.Default.Register<StatusDashboardViewModel>();
            SimpleIoc.Default.Register<CandidateCustomerViewModel>();

            Models.ExchangeModel.LoginModel model = new Models.ExchangeModel.LoginModel();
            //string str = Newtonsoft.Json.JsonConvert.SerializeObject(model);
            model = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginModel>(Properties.Settings.Default.testjson);



        }

        public static void Dispose()
        {
            
        }

    }
}
