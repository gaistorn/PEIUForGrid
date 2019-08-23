using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PEIU.GUI.WebServices;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEIU.GUI.ViewModel
{
    public class CustomerManagerViewModel : ViewModelBase
    {
        private IEnumerable<RegisterViewModel> _accountSource;

        public IEnumerable<RegisterViewModel> AccountSource { get {
                return _accountSource ?? (_accountSource = new List<RegisterViewModel>()
);
            }
            set
            {
                _accountSource = value;
                var group_source = _accountSource.GroupBy(key => (AuthRoles)key.AuthRoles, v => v);
                _accounts = new ObservableCollection<IGrouping<AuthRoles, RegisterViewModel>>(group_source);
                RaisePropertyChanged("Accounts");
            }
        }

        public CustomerManagerViewModel()
        {
            RequestCustomerDataCommand = new RelayCommand(RequestCustomerData);
            RequestCustomerData();
        }

        public void RaiseChangedAccountProperty()
        {
            RaisePropertyChanged("Accounts");
        }

        public RelayCommand RequestCustomerDataCommand
        {
            get;
            private set;
        }

        public async void RequestCustomerData()
        {
            try
            {
                var result = await ContractWebService.RequestCollectionGetMethod<RegisterViewModel>("/api/contract/getcontractorlist");
                AccountSource = result; 
            }
            catch (Exception ex)
            {
               // System.Windows.MessageBox.Show(ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        //public ObservableCollection<RegisterViewModel> Accounts { get => _accounts; set => this.Set(ref _accounts, value); }

        ObservableCollection<IGrouping<AuthRoles, RegisterViewModel>> _accounts;
        public ObservableCollection<IGrouping<AuthRoles, RegisterViewModel>> Accounts
        { get { return _accounts ?? (_accounts = new ObservableCollection<IGrouping<AuthRoles, RegisterViewModel>>()); } }





        public string Test { get; set; } = "HELLO";
    }
}
