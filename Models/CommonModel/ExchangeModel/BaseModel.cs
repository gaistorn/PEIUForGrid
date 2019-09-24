using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PEIU.Models.ExchangeModel
{
    public class BaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
