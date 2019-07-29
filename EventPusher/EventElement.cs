using Newtonsoft.Json.Linq;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PEIU.Hubbub
{
    public class EventElement
    {
        private Dictionary<ushort, ushort> _evtMaps = new Dictionary<ushort, ushort>();

        public void AddEvent(ushort EventCode, ushort Flag)
        {
            if (_evtMaps.ContainsKey(EventCode) == false)
                _evtMaps.Add(EventCode, Flag);
            else
            {
                _evtMaps[EventCode] = Flag;
            }
        }

        public EventSummary Validating(ushort beforeValue, ushort NewValue)
        {
            EventSummary summary = new EventSummary();
            if (beforeValue == NewValue)
                return summary;
            foreach(ushort EventCode in _evtMaps.Keys)
            {
                ushort evt_flag = _evtMaps[EventCode];
                bool beforeFlag = (beforeValue & evt_flag) == evt_flag;
                bool newFlag = (NewValue & evt_flag) == evt_flag;
                if (newFlag)
                    summary.ActiveEvents.Add(EventCode);
                if (beforeFlag == newFlag)
                    continue;
                if (beforeFlag == false && newFlag) // 새로운 이벤트 발생
                    summary.NewEvents.Add(EventCode);
                else if (newFlag == false && beforeFlag) // 복구 이벤트
                    summary.RecoverEvents.Add(EventCode);
            }
            return summary;
        }
        
    }

    
}
