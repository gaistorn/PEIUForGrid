using PEIU.GUI.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PEIU.GUI.View
{
    /// <summary>
    /// CustomerManagerView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CustomerManagerView : UserControl
    {
        public CustomerManagerViewModel ViewModel => (CustomerManagerViewModel)this.DataContext;
        public CustomerManagerView()
        {
            InitializeComponent();
            ViewModel.PropertyChanged += ViewMode_PropertyChanged;
        }

        private void ViewMode_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CustomerFilterText")
            {
                PART_treeView.Filter = ViewModel.CustomerFilterText;
            }
        }

    }
}
