using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PEIU.GUI.WebServices;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PEIU.GUI.ViewModel
{
    public class CustomerManagerViewModel : ViewModelBase, IUpdateWebData, IBedgeMenuModel
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
        public int BedgeCount {  get => _bedgeCount; set => base.Set("BedgeCount", ref _bedgeCount, value); }
        private int _bedgeCount = 0;

        private bool canUpdate = true;
        public bool CanUpdate
        {
            get
            {
                return canUpdate;
            }
            set
            {
                this.Set("CanUpdate", ref canUpdate);
            }
        }

        public string Title => "고객관리";

        public string Tooltip => "고객관리 페이지";

        private View.CustomerManagerView view;
        public object OwnerControl
        {
            get
            {
                if (view == null)
                    view = new View.CustomerManagerView();
                return view ?? (view = new View.CustomerManagerView());
            }
        }

        public CustomerManagerViewModel()
        {
        }

        public void RaiseChangedAccountProperty()
        {
            RaisePropertyChanged("Accounts");
        }

       
        public async Task StartUpdateAsync(CancellationToken cancellationToken)
        {
            try
            {
                var result = await ContractWebService.RequestCollectionGetMethod<RegisterViewModel>("/api/contract/getcontractorlist");
                AccountSource = result;
                BedgeCount = result.Count(x => (AuthRoles)x.AuthRoles == AuthRoles.Candidator);
            }
            catch (Exception ex)
            {
                // System.Windows.MessageBox.Show(ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public object Icon
        {
            get
            {
                MahApps.Metro.IconPacks.PackIconUnicons icon = new MahApps.Metro.IconPacks.PackIconUnicons();
                icon.Kind = MahApps.Metro.IconPacks.PackIconUniconsKind.Adjust;
                return icon;
            }
        }


        //public ObservableCollection<RegisterViewModel> Accounts { get => _accounts; set => this.Set(ref _accounts, value); }

        ObservableCollection<IGrouping<AuthRoles, RegisterViewModel>> _accounts;
        public ObservableCollection<IGrouping<AuthRoles, RegisterViewModel>> Accounts
        { get { return _accounts ?? (_accounts = new ObservableCollection<IGrouping<AuthRoles, RegisterViewModel>>()); } }





        public string Test { get; set; } = "HELLO";


    }
}
