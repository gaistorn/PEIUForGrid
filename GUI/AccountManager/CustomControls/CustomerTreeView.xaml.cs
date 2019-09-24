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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PEIU.GUI.CustomControls
{
    /// <summary>
    /// CustomerTreeView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CustomerTreeView : TreeView
    {


        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); DoFilter(); }
        }

        // Using a DependencyProperty as the backing store for Filter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(string), typeof(CustomerTreeView), new PropertyMetadata(null));



        public CustomerTreeView()
        {
            InitializeComponent();
            
        }

        private void DoFilter()
        {
            //int cnt = 0;
            //foreach(IGrouping<AuthRoles,RegisterViewModel> item in this.Items)
            //{
            //    foreach(RegisterViewModel viewmodel in item)
            //    {
            //        cnt++;
            //        if (cnt % 3 == 0)
            //        {
            //          //  viewmodel.Visibility = Visibility.Collapsed;
            //        }
            //    }
                
            //    //foreach (var child_item in item.)
            //    //{
            //    //    //child_item.Visibility = child_item.Header
            //    //}
            //}
        }
        
    }

}
