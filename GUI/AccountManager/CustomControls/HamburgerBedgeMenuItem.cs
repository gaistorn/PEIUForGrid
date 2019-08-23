using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PEIU.GUI.CustomControls
{
    public class HamburgerBedgeMenuItem : HamburgerMenuIconItem
    {
        public int Bedge
        {
            get { return (int)GetValue(BedgeProperty); }
            set { SetValue(BedgeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bedge.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BedgeProperty =
            DependencyProperty.Register("Bedge", typeof(int), typeof(HamburgerBedgeMenuItem), new PropertyMetadata(3));


    }
}
