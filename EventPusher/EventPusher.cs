using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PEIU.DataServices;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace PEIU.Hubbub
{
    public class EventPusher : MqttFactoryHost
    {
        private readonly LightningDB.LightningEnvironment lmdb_env;
        private LightningDB.LightningDatabase db;
        ILogger<EventPusher> _logger = null;
        private readonly Dictionary<string, EventElement> eventElements = new Dictionary<string, EventElement>();
        
        private readonly bool _initialize;
        public EventPusher(MqttAddress addr, bool Initialize = false) : base(addr)
        {
            _initialize = Initialize;
            lmdb_env = new LightningDB.LightningEnvironment("./Data");
            lmdb_env.MaxDatabases = 2;
            lmdb_env.Open();
            if (Initialize)
                lmdb_env.Flush(true);
            //db = lmdb_env.OpenDatabase("db_event", new DatabaseConfig(DbFlags.Create | DbFlags.IntegerKey));

        }

        public void SetLogger(ILogger<EventPusher> logger)
        {
            _logger = logger;
        }

        public void BeginProcessing()
        {
            txn = lmdb_env.BeginTransaction();
            db = txn.OpenDatabase("db_event", new LightningDB.DatabaseConfiguration { Flags = LightningDB.DatabaseOpenFlags.Create });
            
        }

        private LightningDB.LightningTransaction txn;

        public void AddEvent(string GroupName, ushort Code, ushort Flag)
        {
            if(eventElements.ContainsKey(GroupName) == false)
            {
                eventElements.Add(GroupName, new EventElement());
            }

            eventElements[GroupName].AddEvent(Code, Flag);
        }

        public bool ProcessingEvent(short siteId, string DeviceName, string GroupName, ushort Value, ref EventSummary summary)
        {
            if (eventElements.ContainsKey(GroupName) == false)
                return false;
            string event_unique_id = $"{siteId}.{DeviceName}.{GroupName}";
            byte[] key_ = Encoding.UTF8.GetBytes(event_unique_id);
            byte[] value_ = BitConverter.GetBytes(Value);
            ushort OldValue = 0;
            if(txn.ContainsKey(db, key_) == false)
            {
                txn.Put(db, key_, value_);
                OldValue = 0;
                txn.Commit();
            }
            else
            {
                byte[] db_value_ = null;
                if (txn.TryGet(db, key_, out db_value_) == false)
                {
                    return false;
                }

                OldValue = BitConverter.ToUInt16(db_value_);
                txn.Put(db, key_, value_);
                txn.Commit();
            }

            if (OldValue == Value)
                return false;
             summary = eventElements[GroupName].Validating(OldValue, Value);
            summary.DeviceName = DeviceName;
            summary.GroupName = GroupName;
            summary.SiteId = siteId;
            summary.SetTimestamp(DateTime.Now);
            return summary.HasOccurEvent;

        }


        public async void EventPushing(EventSummary summary)
        {
            if (summary.HasOccurEvent == false)
                return;
            JObject evtRow = summary.CreateJObject();

            _logger?.LogInformation($"RAISE EVENT {evtRow}");

            await PublishAsync(evtRow.ToString());
        }

        public void EndProcessing()
        {
            db.Dispose();
            txn.Dispose();
        }

    }
}
