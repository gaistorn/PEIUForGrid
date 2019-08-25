using GalaSoft.MvvmLight;
using MahApps.Metro.Controls;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PEIU.GUI.CustomControls
{
    public class BedgeMenuItem : MenuItemBase
    {
        public int Bedge
        {
            get { return (int)GetValue(BedgeProperty); }
            set { SetValue(BedgeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bedge.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BedgeProperty =
            DependencyProperty.Register("Bedge", typeof(int), typeof(BedgeMenuItem), new PropertyMetadata(3));
    }

   public class MenuItemBase : HamburgerMenuIconItem
    {
        public ViewModelBase ViewModel
        {
            get { return (ViewModelBase)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ViewModelBase), typeof(MenuItemBase), new PropertyMetadata(null));

        public IMenuModel MenuModel
        {
            get
            {
                return (IMenuModel)ViewModel;
            }
        }


    }
}
