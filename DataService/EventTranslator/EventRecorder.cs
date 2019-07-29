using PEIU.DataServices;
using PEIU.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.Core
{
    public class EventRecorder
    {
        readonly MysqlDataAccess da;
        readonly NLog.ILogger _logger;
        readonly IBackgroundTaskQueue<EventSummary> _queue;
        public EventRecorder(NLog.ILogger logger, IBackgroundTaskQueue<EventSummary> queue, string ConnectionString)
        {
            _logger = logger;
            _queue = queue;
            da = new MysqlDataAccess(ConnectionString);
        }

        private string CreateEventId(NHibernate.IStatelessSession session, EventSummary summary, ushort Code)
        {
            int num = 0;
            ActiveEvent newEvt = null;
            string newEvtId = null;
            while (true)
            {
                newEvtId = $"{summary.SiteId}.{summary.DeviceName}.{Code}.{num++}";
                newEvt = session.Get<ActiveEvent>(newEvtId);
                if (newEvt == null)
                    break;
            }
            return newEvtId;
        }

        private async Task RecoverEventAsync(NHibernate.IStatelessSession session, EventSummary summary, ushort Code, CancellationToken token)
        {
            int num = 0;
            ActiveEvent newEvt = null;
            string newEvtId = null;
            while (true)
            {
                newEvtId = $"{summary.SiteId}.{summary.DeviceName}.{Code}.{num++}";
                newEvt = session.Get<ActiveEvent>(newEvtId);
                if (newEvt != null && newEvt.HasRecovered == false)
                {
                    newEvt.RecoverTimestamp = summary.GetTimestamp();
                    newEvt.HasRecovered = true;
                    await session.UpdateAsync(newEvt, token);
                }
                else if (newEvt == null)
                    break;
            }
        }

        public async Task RunAsync(CancellationToken token)
        {
            while(token.IsCancellationRequested == false)
            {
                EventSummary summary = await _queue.DequeueAsync(token);
                try
                {
                    using (var session = da.SessionFactory.OpenStatelessSession())
                    using (var txn = session.BeginTransaction())
                    {
                        foreach (ushort Code in summary.RecoverEvents)
                        {
                            await RecoverEventAsync(session, summary, Code, token);
                        }


                        foreach (ushort Code in summary.NewEvents)
                        {
                            EventMap map = await session.GetAsync<EventMap>(Code, token);
                            if (map == null)
                            {
                                _logger.Warn($"DB상에 존재하지 않는 이벤트 코드입니다. 이벤트 코드:{Code}");
                                continue;
                            }

                            DeviceEvent ae = new DeviceEvent();

                            byte[] descBuffer = Encoding.UTF8.GetBytes(map.Description);
                            ae.DeviceName = summary.DeviceName;
                            ae.EventId = CreateEventId(session, summary, Code);
                            ae.EventCode = Code;
                            ae.OccurTimestamp = summary.GetTimestamp();
                            ae.siteId = summary.SiteId;
                            await session.InsertAsync(ae, token);
                        }

                        
                        await txn.CommitAsync(token);
                    }
                }
                catch(Exception ex)
                {
                    _logger.Error(ex);
                }
            }
            _logger.Info("Abort RunAsync");
        }
    }
}
