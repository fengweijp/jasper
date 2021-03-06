﻿using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus.Runtime;

namespace Jasper.Bus
{
    [Obsolete("Eliminate this in favor of just editing the Envelope directly")]
    public class RequestOptions
    {
        public TimeSpan Timeout = 10.Minutes();
        public Uri Destination = null;
    }

    public interface IServiceBus
    {
        /// <summary>
        /// Loosely-coupled Request/Reply pattern
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="options">Override the timeout and destination</param>
        /// <returns></returns>
        Task<TResponse> Request<TResponse>(object request, RequestOptions options = null);

        /// <summary>
        /// Publish a message to all known subscribers
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Send<T>(T message);

        /// <summary>
        /// Send a message with explict control overrides to the Envelope
        /// </summary>
        /// <param name="message"></param>
        /// <param name="customize"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Send<T>(T message, Action<Envelope> customize);

        /// <summary>
        /// Send to a specific destination rather than running the routing rules
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination">The destination to send to</param>
        /// <param name="message"></param>
        Task Send<T>(Uri destination, T message);

        /// <summary>
        /// Invoke consumers for the relevant messages managed by the current
        /// service bus instance. This happens immediately and on the current thread.
        /// Error actions will not be executed and the message consumers will not be retried
        /// if an error happens.
        /// </summary>
        Task Invoke<T>(T message);

        /// <summary>
        /// Enqueues the message into the default loopback queue for this application
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task Enqueue<T>(T message);

        /// <summary>
        /// Send a message that should be executed at the given time
        /// </summary>
        /// <param name="message"></param>
        /// <param name="time"></param>
        /// <typeparam name="T"></typeparam>
        Task DelaySend<T>(T message, DateTime time);

        /// <summary>
        /// Send a message that should be executed after the given delay
        /// </summary>
        /// <param name="message"></param>
        /// <param name="delay"></param>
        /// <typeparam name="T"></typeparam>
        Task DelaySend<T>(T message, TimeSpan delay);

        /// <summary>
        /// Send a message and await an acknowledgement that the
        /// message has been processed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendAndWait<T>(T message);

        /// <summary>
        /// Send a message to a specific destination and await an acknowledgment
        /// that the message has been processed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination">The destination to send to</param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendAndWait<T>(Uri destination, T message);
    }
}
