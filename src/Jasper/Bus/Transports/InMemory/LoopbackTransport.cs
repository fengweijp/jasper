﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.Transports.InMemory
{
    public class LoopbackTransport : ITransport
    {
        private readonly LoopbackSettings _settings;
        private Uri _replyUri;
        private readonly ILoopbackQueue _queue;

        public LoopbackTransport(LoopbackSettings settings, ILoopbackQueue queue)
        {
            _settings = settings;
            _queue = queue;
        }

        public string Protocol => "loopback";

        public Task Send(Envelope envelope, Uri destination)
        {
            return _queue.Send(envelope, destination);
        }

        public void Start(IHandlerPipeline pipeline, ChannelGraph channels)
        {
            channels.AddChannelIfMissing(Retries);

            var nodes = channels.Where(x => x.Uri.Scheme == Protocol).ToList();
            if (!nodes.Any()) return;

            var replyNode = nodes.FirstOrDefault(x => x.Incoming) ??
                            channels.AddChannelIfMissing(_settings.DefaultReplyUri);

            replyNode.Incoming = true;
            _replyUri = replyNode.Uri;

            nodes.Add(replyNode);

            _queue.Start(nodes);

            foreach (var node in nodes)
            {
                node.Destination = node.Uri;
                node.ReplyUri = _replyUri;
                node.Sender = new LoopbackSender(node.Uri, _queue);

                _queue.ListenForMessages(node, pipeline, channels);

            }
        }

        public Uri DefaultReplyUri()
        {
            return _replyUri;
        }

        public void Dispose()
        {

        }

        public static readonly Uri Delayed = "loopback://delayed".ToUri();
        public static readonly Uri Retries = "loopback://retries".ToUri();
    }
}