using GalaSoft.MvvmLight;
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
        private ObservableCollection<RegisterViewModel> _accounts;

        public ObservableCollection<RegisterViewModel> Accounts { get => _accounts; set => this.Set(ref _accounts, value); }

        public string Test { get; set; } = "HELLO";
    }
}
