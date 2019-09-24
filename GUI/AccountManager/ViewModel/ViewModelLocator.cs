using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using PEIU.GUI.Models;
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
        public static CommonServiceModel CommonServiceModelStatic => SimpleIoc.Default.GetInstance<CommonServiceModel>();
        public static MainWindowViewModel MainStatic => SimpleIoc.Default.GetInstance<MainWindowViewModel>();
        public static CustomerManagerViewModel CustomersStatic => SimpleIoc.Default.GetInstance<CustomerManagerViewModel>();

        public static ContractorViewModel ContractorStatic => SimpleIoc.Default.GetInstance<ContractorViewModel>();

       // public static AggregatorViewModel AggregatorViewModelStatic = SimpleIoc.Default.GetInstance<AggregatorViewModel>();

        static ViewModelLocator()
        {
        }

        public CommonServiceModel CommonModel => CommonServiceModelStatic;

        public MainWindowViewModel Main => MainStatic;
        public CustomerManagerViewModel Customers => CustomersStatic;

       // public AggregatorViewModel Aggregators => AggregatorViewModelStatic;

        public ContractorViewModel ContractorViewModel => ContractorStatic;

        public StatusDashboardViewModel StatusDashboard => SimpleIoc.Default.GetInstance<StatusDashboardViewModel>();

       public ContractorCustomerViewModel Contractor => SimpleIoc.Default.GetInstance<ContractorCustomerViewModel>();

        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<CommonServiceModel>();

            //SimpleIoc.Default.Register<Models.IBaseModel, MainWindowViewModel>();
            SimpleIoc.Default.Register<ContractorViewModel>();
            SimpleIoc.Default.Register<MainWindowViewModel>();
            SimpleIoc.Default.Register< CustomerManagerViewModel>();
            //SimpleIoc.Default.Register<StatusDashboardViewModel>();
            SimpleIoc.Default.Register<StatusDashboardViewModel>();
            SimpleIoc.Default.Register<ContractorCustomerViewModel>();
          //  SimpleIoc.Default.Register<AggregatorViewModel>();

            //Models.ExchangeModel.LoginModel model = new Models.ExchangeModel.LoginModel();
            //string str = Newtonsoft.Json.JsonConvert.SerializeObject(model);
            //model = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginModel>(Properties.Settings.Default.testjson);



        }

        public static void Dispose()
        {
            
        }

    }
}
