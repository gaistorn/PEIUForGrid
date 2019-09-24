using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
using PEIU.GUI.ViewModel;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PEIU.GUI.View
{
    /// <summary>
    /// ContractorApprovalView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContractorApprovalView : MetroWindow
    {
        ContractorViewModel ViewModel => DataContext as ContractorViewModel;
        Dictionary<int, (Border border, TextBlock txt)> maps = null;

       
        

        public ContractorApprovalView()
        {
            InitializeComponent();
            StatusUpdate();
        }


        private void StatusUpdate()
        {
            maps = new Dictionary<int, (Border border, TextBlock txt)>()
            {
                { (int)ContractStatusCodes.Signing, (border : this.bSigning, txt:txtSigning) },
                { (int)ContractStatusCodes.Cantacting, (border : this.bConsult, txt:txtConsult) },
                { (int)ContractStatusCodes.Explorating, (border : this.bFieldWork, txt:txtFieldWork ) },
                { (int)ContractStatusCodes.WaitingForApprval, (border : this.bConference, txt:txtConference) },
                { (int)ContractStatusCodes.Activating, (border : this.bConsult, txt:txtCompleted) }

            };
            if(ViewModel.Contractor.ContractStatus != ContractStatusCodes.Explorating)
            {
                cbSite.Visibility = tbSite.Visibility = Visibility.Collapsed;
            }
            for(int i= (int)ContractStatusCodes.Signing;i<= (int)ViewModel.Contractor.ContractStatus; i++)
            {
                if(maps.ContainsKey(i))
                {
                    maps[i].border.Background = new SolidColorBrush(Colors.GreenYellow);
                    if (i != (int)ViewModel.Contractor.ContractStatus)
                        maps[i].txt.Text = "결제완료";
                    else
                        maps[i].txt.Text = "결제대기중";
                }
            }
        }
    }
}
