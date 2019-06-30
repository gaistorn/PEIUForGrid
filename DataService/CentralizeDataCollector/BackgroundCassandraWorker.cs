using Cassandra;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PEIU.DataServices
{
    public class BackgroundCassandraWorker
    {
        IBackgroundTaskQueue<JObject> _queue;
        CassandraConfiguration _config;
        ISession _session;
        ILogger _logger;
        public CancellationToken Token { get; private set; }

        public event RunWorkerCompletedEventHandler RunWorkerCompleted;

        const string cql_pcs_insert_query = @"INSERT INTO PCS_ANALOGUE (deviceid,siteid,updatetime,
            acgridcrtr,acgridcrts,acgridcrtt,acgridcrt, acgridpwr,
            acgridvltr,acgridvlts,acgridvltt,acgridvlt,
            freq, actpwrkw, rctpwrkw, 
            pwrfact, actcmdlimitlowdhg, actcmdlimithighdhg, actcmdlimitlowchg, actcmdlimithighchg) VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";




        public BackgroundCassandraWorker(ILogger logger, CancellationTokenSource cancellationTokenSource, CassandraConfiguration config, IBackgroundTaskQueue<JObject> queue)
        {
            _queue = queue;
            _config = config;
            _logger = logger;
            Token = cancellationTokenSource.Token;
        }

        public  async Task RunWorkerAsync()
        {
            var cluster = Cluster.Builder()
                    .AddContactPoints(_config.Hosts)
                    .Build();
            // Connect to the nodes using a keyspace

            _session = await cluster.ConnectAsync("siteresources").ConfigureAwait(false);
            _logger.Info("Connect cassandra");
            _logger.Warn("Warning");
            await _session.UserDefinedTypes.DefineAsync(
                UdtMap.For<RstType>(udtName:"rst", keyspace: "siteresources")
                );
            var pcs_insert_statement = await _session.PrepareAsync(cql_pcs_insert_query).ConfigureAwait(false);
            DateTime nextExecuteTime = DateTime.Now.Add(_config.WaitForBatch);
            List<Task> insertTasks = new List<Task>();
            
            while (true)
            {
                if (Token.IsCancellationRequested)
                    break;
                Token.ThrowIfCancellationRequested();
                JObject obj = await _queue.DequeueAsync(Token);
                if (obj.ContainsKey(COMMON_KEYS.GROUPID_KEY) == false)
                    continue;
                int group_id = obj[COMMON_KEYS.GROUPID_KEY].Value<int>();
                if(group_id == 1) // PCS
                {
                    insertTasks.Add(ExecuteInsertPcs(obj, pcs_insert_statement, _session));
                }
                if(insertTasks.Count >= _config.BatchCount || DateTime.Now >= nextExecuteTime)
                {
                    await Task.WhenAll(insertTasks).ConfigureAwait(false);
                    _logger.Info($"Insert rows({insertTasks.Count}) data");
                    insertTasks.Clear();
                    nextExecuteTime = DateTime.Now.Add(_config.WaitForBatch);
                }

                await Task.Delay(10);
            }

            await cluster.ShutdownAsync().ConfigureAwait(false);
            if (RunWorkerCompleted != null)
                RunWorkerCompleted(this, new RunWorkerCompletedEventArgs(null, null, Token.IsCancellationRequested));



        }

        private async Task ExecuteInsertPcs(JObject obj, PreparedStatement _ps, ISession session)
        {
            string ts_str = obj[COMMON_KEYS.TIMESTAMP_KEY].Value<string>();
            DateTime timestamp = DateTime.Now;
            DateTime.TryParseExact(ts_str, "yyyyMMddHHmmss", CultureInfo.CurrentCulture, DateTimeStyles.None, out timestamp);
            RstType acGridCrt = new RstType();
            acGridCrt.R = obj["acGridCrtR"].Value<float>();
            acGridCrt.S = obj["acGridCrtS"].Value<float>();
            acGridCrt.T = obj["acGridCrtT"].Value<float>();

            RstType acGridVlt = new RstType();
            acGridVlt.R = obj["acGridVltR"].Value<float>();
            acGridVlt.S = obj["acGridVltS"].Value<float>();

            acGridVlt.T = obj["acGridVltT"].Value<float>();

            RstType acVlt = new RstType();
            acVlt.R = obj["acGridVlt"].Value<float>();

            float ac_pwr = obj["acGridPwr"].Value<float>();
            double freq = obj["freq"].Value<double>();
            float active_pwr = obj["actPwrKw"].Value<float>();
            float reactv_pwr = obj["rctPwrKw"].Value<float>();
            float pwr_fct = obj["pwrFact"].Value<float>();
            float actCmdLimitLowDhg = obj["actCmdLimitLowDhg"].Value<float>();
            float actCmdLimitHighDhg = obj["actCmdLimitHighDhg"].Value<float>();
            float actCmdLimitLowChg = obj["actCmdLimitLowChg"].Value<float>();
            float actCmdLimitHighChg = obj["actCmdLimitHighChg"].Value<float>();
            string deviceKey = obj[COMMON_KEYS.DEVICEID_KEY].Value<string>();
            int siteId = obj[COMMON_KEYS.SITEID_KEY].Value<int>();


            var bs = _ps.Bind(deviceKey, siteId, timestamp,
                 acGridCrt.R, acGridCrt.S, acGridCrt.T, acGridCrt, ac_pwr,
                 acGridVlt.R, acGridVlt.S, acGridVlt.T, acGridVlt,
                 freq, active_pwr, reactv_pwr,
                 pwr_fct, actCmdLimitLowDhg, actCmdLimitHighDhg, actCmdLimitLowChg, actCmdLimitHighChg);


            await session.ExecuteAsync(bs).ConfigureAwait(false);
        }
    }
}
