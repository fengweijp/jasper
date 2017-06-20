﻿using System;
using System.Threading.Tasks;
using Jasper.Bus.Delayed;
using Jasper.Bus.Queues;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports.LightningQueues
{
    public class TransactionCallback : IMessageCallback
    {
        private readonly Message _message;
        private readonly IQueueContext _context;

        public TransactionCallback(IQueueContext context, Message message)
        {
            _context = context;
            _message = message;
        }

        public void MarkSuccessful()
        {
            _context.SuccessfullyReceived();
            _context.CommitChanges();
        }

        public void MarkFailed(Exception ex)
        {
            _context.ReceiveLater(DateTimeOffset.Now);
            _context.CommitChanges();
        }

        public Task MoveToDelayedUntil(Envelope envelope, IDelayedJobProcessor delayedJobs, DateTime time)
        {
            delayedJobs.Enqueue(time, envelope);

            // TODO -- will be smarter later if you're using the LMDB backed delayed jobs
//            _context.ReceiveLater(time.ToUniversalTime() - DateTime.UtcNow);
//            _context.CommitChanges();
            return Task.CompletedTask;
        }

        public void MoveToErrors(ErrorReport report)
        {
            var message = new Message
            {
                Id = _message.Id,
                Data = report.Serialize(),
                Headers = _message.Headers
            };

            message.Headers.Add("ExceptionType", report.ExceptionType);
            message.Queue = LightningQueue.ErrorQueueName;

            _context.Enqueue(message);
            MarkSuccessful();
        }

        public Task Requeue(Envelope envelope)
        {
            var copy = _message.Copy();
            copy.Id = MessageId.GenerateRandom();
            copy.Queue = _message.Queue;
            _context.Enqueue(copy);
            MarkSuccessful();
            return Task.CompletedTask;
        }

        public Task Send(Envelope envelope)
        {
            var uri = new LightningUri(envelope.Destination);

            var message = new OutgoingMessage
            {
                Id = MessageId.GenerateRandom(),
                Data = envelope.Data,
                Headers = envelope.Headers,
                SentAt = DateTime.UtcNow,
                Destination = envelope.Destination,
                Queue = uri.QueueName
            };

            message.TranslateHeaders();

            _context.Send(message);
            return Task.CompletedTask;
        }

        public bool SupportsSend { get; } = true;
    }
}