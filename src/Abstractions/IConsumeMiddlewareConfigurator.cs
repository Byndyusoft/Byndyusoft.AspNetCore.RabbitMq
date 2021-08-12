﻿namespace Byndyusoft.Net.RabbitMq.Abstractions
{
    /// <summary>
    ///    Api for builing pipeline for consuming message
    /// </summary>
    /// <typeparam name="TMessage">Incoming message type</typeparam>
    public interface IConsumeMiddlewareConfigurator<TMessage> : IConsumeErrorPipeConfigurator<TMessage> where TMessage : class
    {
        /// <summary>
        ///     Adds middleware to consuming pipeline
        /// </summary>
        /// <typeparam name="TMiddleware">Middleware type</typeparam>
        IConsumeMiddlewareConfigurator<TMessage> Wrap<TMiddleware>() where TMiddleware : IConsumeMiddleware<TMessage>;
    }
}
