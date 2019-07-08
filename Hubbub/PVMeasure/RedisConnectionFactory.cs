using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PVMeasure
{
    public interface IRedisConnectionFactory
    {
        ConnectionMultiplexer Connection();
    }

    public class RedisConnectionFactory : IRedisConnectionFactory
    {
        /// <summary>
        ///     The _connection.
        /// </summary>
        private readonly Lazy<ConnectionMultiplexer> _connection;


        public RedisConnectionFactory(RedisConfiguration redis)
        {
            //Console.WriteLine(redis.Value.ConfigurationOptions.ToString());
            
            this._connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(redis.ConfigurationOptions));

            //_connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(redis.ConfigurationOptions));
        }

        public ConnectionMultiplexer Connection()
        {
            return this._connection.Value;
        }
    }
}
