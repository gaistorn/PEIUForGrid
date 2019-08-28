using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PEIU.Models
{
    public class ListViewModel<T> : ListCollectionView
    {
        public ListViewModel(IList<T> list) : base(list.ToList())
        {

        }

        public ListViewModel<T> SetFilterColumn(string columnName)
        {
            //view.GroupDescriptions.Add(new PropertyGroupDescription("Country"));
            var p = new PropertyGroupDescription(columnName);
            this.GroupDescriptions.Add(p);
            return this;
        }

        public ListViewModel<T> SetFilterColumn<CType>(string columnName) where CType : IValueConverter
        {
            var p = new PropertyGroupDescription(columnName);
            p.Converter = Activator.CreateInstance<CType>();
            this.GroupDescriptions.Add(p);
            return this;
        }

        new public NotifyCollectionChangedEventHandler CollectionChanged;
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }
    }
}
