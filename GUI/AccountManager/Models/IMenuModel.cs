using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PEIU.Models
{

    public interface IBaseModel { }
    public interface IMenuModel : IBaseModel
    {
        string Title { get; }
        string Tooltip { get; }

        object Icon { get; }

        object OwnerControl { get; }
    }

    public interface IBedgeMenuModel : IMenuModel
    {
        int BedgeCount { get; }
    }
}
