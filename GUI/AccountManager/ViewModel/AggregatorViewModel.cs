using GalaSoft.MvvmLight;
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
    public class AggregatorViewModel : ViewModelBase, IUpdateWebData, IBedgeMenuModel
    {
        public bool CanUpdate => throw new NotImplementedException();

        public int BedgeCount => throw new NotImplementedException();

        public string Title => throw new NotImplementedException();

        public string Tooltip => throw new NotImplementedException();

        public object Icon => throw new NotImplementedException();

        public object OwnerControl => throw new NotImplementedException();

        public Task StartUpdateAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
