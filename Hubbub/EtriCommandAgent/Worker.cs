using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using StackExchange.Redis;

namespace EtriCommandAgent
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly MysqlDataAccessSingleton mysqlDataAccess;
        readonly IDatabaseAsync redisdatabaseAsync;
        readonly EtriCommandPublisher publisher;
        private DateTime LastReadingCommandDate;

        public Worker(ILogger<Worker> logger, MysqlDataAccessSingleton mysqlDataAccess, ConnectionMultiplexer connectionMultiplexer, EtriCommandPublisher publisher)
        {
            _logger = logger;
            this.mysqlDataAccess = mysqlDataAccess;
            redisdatabaseAsync = connectionMultiplexer.GetDatabase(1);
            this.publisher = publisher;
        }

        private async Task SOCProtectFunction(CancellationToken stoppingToken)
        {
            for (int pcsNo = 1; pcsNo <= 4; pcsNo++)
            {
                if (await AutoModeCheck(pcsNo) == false)
                    continue;
                var pcs_values = await GetDeviceValues(pcsNo, "bms_soc", "socMn");
                float soc = pcs_values[0];
                float soc_min = pcs_values[1];
                if(soc < soc_min)
                {
                    _logger.LogWarning($"[���] [PCS{pcsNo}] ���� SOC({soc})�� SOC MIN({soc_min}) �̸����� �Ǿ����ϴ�. ������ ���� ����� �����մϴ�");
                    await publisher.PublishAsync(stoppingToken, pcsNo, 190, 10);
                }
            }
        }

        private async Task<bool> AutoModeCheck(int PcsNo)
        {
            var flag = await GetLocalRemote(PcsNo);
            return flag == 1;

        }

        private async Task<bool> ValidatingSOC(int PcsNo, float Command)
        {
            var pcs_values = await GetDeviceValues(PcsNo, "bms_soc", "socMn", "socMx");
            float soc = pcs_values[0];
            float soc_min = pcs_values[1];
            float soc_max = pcs_values[2];

            if (await AutoModeCheck(PcsNo) == false)
            {
                _logger.LogWarning($"[���] [PCS{PcsNo}] ���� Manual ����Դϴ�");
                return false;
            }

            if (Command > 0 && soc >= soc_max)
            {
                _logger.LogWarning($"[���] [PCS{PcsNo}] ���� ���� ���({Command})�� ��ҵǾ����ϴ�. ����) SOC({soc})�� �ִ� SOC���� ({soc_max})�� �ʰ��߽��ϴ�");
                return false;
            }
            else if (Command < 0 && soc <= soc_min)
            {
                _logger.LogWarning($"[���] [PCS{PcsNo}] ���� ���� ���({Command})�� ��ҵǾ����ϴ�. ����) SOC({soc})�� �ִ� SOC���� ({soc_max})�� �̸��Դϴ�");
                return false;
            }
            return true;
        }

        private async Task<float> ValidatingPV(int PcsNo, float Command)
        {
            var pcs_values = await GetDeviceValues(PcsNo, "ID_POWER");
            float pv_power = pcs_values[0];

            // ��������� ���
            if (Command > 0)
            {
                float MinTarget = Math.Min(Command, (94 - pv_power));
                return MinTarget;
            }
            else if (Command < 0) // ������� �� ���
            {
                float MaxTarget = Math.Max(Command, (pv_power * -1));
                return MaxTarget;
            }
            else return 0;
        }

        private async Task SendCommandWhenOK(bool IsOK, int pcsNo, float etri_command, CancellationToken stoppingToken)
        {
            try
            {
                if (IsOK == false)
                {
                    //await Task.CompletedTask;
                    return;
                }
                float cmdValue = await ValidatingPV(1, etri_command);
                ushort cmdUshort = (ushort)(cmdValue * 10);
                await publisher.PublishAsync(stoppingToken, pcsNo, 190, cmdUshort);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (IStatelessSession session = mysqlDataAccess.GetStatelessSession())
                    using (var transaction = session.BeginTransaction())
                    {

                        var command = await session.CreateCriteria<TbCommand>()
                            // .Add(Restrictions.Ge("date", "2019-10-01 15:18:00"))
                            // .AddOrder(NHibernate.Criterion.Order.Desc("date"))
                            .UniqueResultAsync<TbCommand>();
                        if (command == null || command.Date == LastReadingCommandDate)
                        {
                            /// ���� ����� ������ SOC �����ؼ� ��� ����
                            await SOCProtectFunction(stoppingToken);
                            continue;
                        }

                        LastReadingCommandDate = command.Date;

                        _logger.LogInformation($"command pcs1: {command.Ess1} pcs2: {command.Ess2} pcs3: {command.Ess3} pcs4: {command.Ess4}");
                        /// 1�ܰ� SOC ���� ������� üũ
                        bool[] ValidSocs = new bool[]
                        {
                    await ValidatingSOC(1, command.Ess1),
                    await ValidatingSOC(2, command.Ess2),
                    await ValidatingSOC(3, command.Ess3),
                    await ValidatingSOC(4, command.Ess4)
                        };


                        bool pcs1_ok = ValidSocs[0];
                        bool pcs2_ok = ValidSocs[1];
                        bool pcs3_ok = ValidSocs[2];
                        bool pcs4_ok = ValidSocs[3];


                        await SendCommandWhenOK(ValidSocs[0], 1, command.Ess1, stoppingToken);
                        await SendCommandWhenOK(ValidSocs[1], 2, command.Ess2, stoppingToken);
                        await SendCommandWhenOK(ValidSocs[2], 3, command.Ess3, stoppingToken);
                        await SendCommandWhenOK(ValidSocs[3], 4, command.Ess4, stoppingToken);
                        await Task.Delay(1000, stoppingToken);
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }

        }

        private async Task<float[]> GetDeviceValues(int PcsNo, params RedisValue[] FieldNames)
        {
            string redisKey = $"6.JeJuGridPcs{PcsNo}";
            var results = (RedisValue[])await redisdatabaseAsync.HashGetAsync(redisKey, FieldNames);
            return results.Select(x => (float)x).ToArray();
        }

        private async Task<int> GetLocalRemote(int PcsNo)
        {
            string redisKey = $"JeJuGridPcs{PcsNo}";
            var results = await redisdatabaseAsync.HashGetAsync(redisKey, "LocalRemote");
            return (int)results;
        }

        private async Task<int> GetOperateStatus(int PcsNo, OperateProperty property)
        {
            string redisKey = $"JeJuGridPcs{PcsNo}.40181.{(int)property}";
            int v = (int)await redisdatabaseAsync.HashGetAsync(redisKey, "Status");
            return v;

        }
    }
}
