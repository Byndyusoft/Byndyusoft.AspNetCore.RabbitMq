﻿using System;
using System.Collections.Generic;

namespace Byndyusoft.Net.RabbitMq.Models
{
    /// <summary>
    ///     Connection and topology configuration for RabbintMq
    /// </summary>
    public sealed class RabbitMqConfiguration
    {
        /// <summary>
        ///     Connection string to RabbitMq
        /// </summary>
        public string  ConnectionString { get; set; }

        /// <summary>
        ///     Configuration of exchanges of binded queues
        /// </summary>
        public Dictionary<string, ExchangeConfiguration> ExchangeConfigurations { get; }

        /// <summary>
        ///     Ctor
        /// </summary>
        public RabbitMqConfiguration()
        {
            ExchangeConfigurations = new Dictionary<string, ExchangeConfiguration>();
        }

        /// <summary>
        ///      Add exchange configuration
        /// </summary>
        /// <param name="exchangeName"></param>
        public void AddExchange(string exchangeName)
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(exchangeName));

            if (ExchangeConfigurations.ContainsKey(exchangeName))
            {
                throw new Exception($"Exchange {exchangeName} has been already added");
            }

            ExchangeConfigurations.Add(exchangeName, new ExchangeConfiguration(exchangeName));
        }
    }
}