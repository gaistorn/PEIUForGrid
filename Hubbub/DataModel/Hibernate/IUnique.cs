using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    public abstract class DataObject
    {
        public virtual string Uniqueid { get; set; }
        public void CreateUniqueKey()
        {
            Uniqueid = string.Format("{0:yyyy}{0:mm}{0:dd}{0:H}{0:MM}", DateTime.Now);
        }

    }
}
