using MahApps.Metro.Controls;
using PEIU.GUI.CustomControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PEIU.GUI.Selector
{
    public  class MainHamburgerMenuItemTempleteSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement elemnt = container as FrameworkElement;
            HamburgerMenuIconItem user = item as HamburgerMenuIconItem;
            if (user is HamburgerBedgeMenuItem)
            {
                return elemnt.FindResource("BedgeMenuItemTemplate") as DataTemplate;
            }
            else
            {
                return elemnt.FindResource("MenuItemTemplate") as DataTemplate;
            }
        }
    }
}
