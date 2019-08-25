using GalaSoft.MvvmLight;
using PEIU.GUI.WebServices;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PEIU.GUI.ViewModel
{
    public class StatusDashboardViewModel : ViewModelBase, IUpdateWebData, IMenuModel
    {
        public string Title => "현황";

        public string Tooltip => "전체 현황에 대한 페이지";

        private Control _pageControl;
        public object OwnerControl
        {
            get
            {
                return _pageControl ?? (_pageControl = new UserControl());
            }
        }

        public object Icon
        {
            get
            {
                MahApps.Metro.IconPacks.PackIconUnicons icon = new MahApps.Metro.IconPacks.PackIconUnicons();
                icon.Kind = MahApps.Metro.IconPacks.PackIconUniconsKind.Microphone;
                return icon;
            }
        }

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

        public async Task StartUpdateAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Update StatusDashBoard");
            await Task.Delay(3000);
        }
    }
}
