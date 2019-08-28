using GalaSoft.MvvmLight;//
using PEIU.GUI.View;
using PEIU.GUI.WebServices;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.GUI.ViewModel
{
    public class CandidateCustomerViewModel : ViewModelBase, IUpdateWebData, IBedgeMenuModel
    {
        public bool CanUpdate => true;

        public int BedgeCount { get => _bedgeCount; set => base.Set("BedgeCount", ref _bedgeCount, value); }
        private int _bedgeCount = 0;

        public string Title => "가입자관리";

        public string Tooltip => "가입자 관리 페이지";
       // <iconPacks:PackIconMaterial Kind = "AccountCardDetailsOutline" />
        public object Icon
        {
            get
            {
                MahApps.Metro.IconPacks.PackIconMaterial icon = new MahApps.Metro.IconPacks.PackIconMaterial();
                icon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.AccountCardDetailsOutline;
                return icon;
            }
        }

        private CandidateCustomerView view;
        public object OwnerControl => view ?? (view = new CandidateCustomerView());

        public Task StartUpdateAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
