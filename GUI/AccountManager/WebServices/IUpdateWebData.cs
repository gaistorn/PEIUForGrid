using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.GUI.WebServices
{
    public interface IUpdateWebData : IBaseModel
    {
        //void UpdateDatasource(string PropertyName);
        bool CanUpdate { get; }
        Task StartUpdateAsync(CancellationToken cancellationToken);
    }
}
