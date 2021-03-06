﻿using System;
using System.Collections.Generic;
using Baseline;
using Baseline.Dates;
using Jasper.Bus.ErrorHandling;
using Jasper.Testing.Bus.Runtime;
using Jasper.Testing.Bus.Samples;
using Jasper.Util;
using Microsoft.Extensions.Configuration;

namespace Jasper.Testing.Samples
{
    // SAMPLE: configuring-messaging-with-JasperRegistry
    public class MyMessagingApp : JasperRegistry
    {
        public MyMessagingApp()
        {
            // Configure handler policies
            Handlers.DefaultMaximumAttempts = 3;
            Handlers.OnException<SqlException>().RetryLater(3.Seconds());

            // Declare published messages
            Publish.Message<Message1>().To("tcp://server1:2222");

            // Register to receive messages
            Subscribe.At("tcp://loadbalancer1:2233");
            Subscribe.To<Message2>();
            Subscribe.To(type => type.IsInNamespace("MyMessagingApp.Incoming"));

            // Configure the built in transports
            Transports.Lightweight.ListenOnPort(2233);
            Transports.Durable.Disable();
        }
    }
    // ENDSAMPLE


    // SAMPLE: MyListeningApp
    public class MyListeningApp : JasperRegistry
    {
        public MyListeningApp()
        {
            // Use the simpler, but transport specific syntax
            // to just declare what port the transport should use
            // to listen for incoming messages
            Transports.Lightweight.ListenOnPort(2233);

            // or use a Uri to declare both the transport type and port
            Transports.ListenForMessagesFrom("tcp://localhost:2233");
        }
    }
    // ENDSAMPLE


    // SAMPLE: configuring-bus-application-with-settings
    public class MySettings
    {
        public int LightweightPort { get; set; } = 2222;
        public Uri DefaultListener { get; set; } = "durable://localhost:2223".ToUri();

        public int MaximumSendAttempts { get; set; } = 5;
    }

    public class MyAppWithSettings : JasperRegistry
    {
        public MyAppWithSettings()
        {
            Configuration
                .AddEnvironmentVariables()
                .AddJsonFile("appSettings.json");

            Settings.With<MySettings>(_ =>
            {
                Transports.Lightweight
                    .ListenOnPort(_.LightweightPort)
                    .MaximumSendAttempts(_.MaximumSendAttempts);

                Transports.ListenForMessagesFrom(_.DefaultListener);

            });
        }
    }
    // ENDSAMPLE


    // SAMPLE: configuring-via-uri-lookup
    public class MyAppUsingUriLookups : JasperRegistry
    {
        public MyAppUsingUriLookups()
        {
            Configuration
                .AddInMemoryCollection(new Dictionary<string, string>{{"incoming", "tcp://server3:2000"}});

            // This usage assumes that there is a value in the configuration
            // with the key "incoming" that corresponds to a Uri
            Transports.ListenForMessagesFrom("config://incoming");
        }
    }
    // ENDSAMPLE

    // SAMPLE: LightweightTransportApp
    public class LightweightTransportApp : JasperRegistry
    {
        public LightweightTransportApp()
        {
            // Nevermind, just disable it altogether
            Transports.Durable.Disable();

            // Set up a listener (this is optional)
            Transports.Lightweight.ListenOnPort(4000);

            // Make it even faster by discarding any message that fails
            Transports.Lightweight.MaximumSendAttempts(1);

            // Configure the maximum number of threads dedicated to processing
            // messages being received by queue. 5 is the default
            Transports.Lightweight.DefaultQueue.MaximumParallelization(5);
            Transports.Lightweight.Queue("important").MaximumParallelization(10);
            Transports.Lightweight.Queue("lowpriority").MaximumParallelization(3);
            Transports.Lightweight.Queue("control").Sequential();


            // Or, listen by Uri
            // This directs Jasper to listen for messages at port 2200
            // with the lightweight transport
            Transports.ListenForMessagesFrom("tcp://localhost:2200");


            // Registering a subscription to Message1 that should be
            // delivered to a load balancer Uri at port 2200 and the "important"
            // queue
            Subscribe.To<Message1>().At("tcp://loadbalancer:2200/important");

            // Publish the message Message2 to the DNS entry "remoteserver"
            Publish.Message<Message2>().To("tcp://remoteserver:2201");
        }
    }
    // ENDSAMPLE

    // SAMPLE: DurableTransportApp
    public class DurableTransportApp : JasperRegistry
    {
        public DurableTransportApp()
        {
            // Nevermind, just disable it altogether
            Transports.Durable.Disable();

            // Set up a listener (this is optional)
            Transports.Durable.ListenOnPort(4000);

            // Throw away messages faster because the default is 100
            // attempts
            Transports.Durable.MaximumSendAttempts(100);

            // Configure the maximum number of threads dedicated to processing
            // messages being received by queue. 5 is the default
            Transports.Durable.DefaultQueue.MaximumParallelization(5);
            Transports.Durable.Queue("important").MaximumParallelization(10);
            Transports.Durable.Queue("lowpriority").MaximumParallelization(3);
            Transports.Durable.Queue("control").Sequential();


            // Or, listen by Uri
            // This directs Jasper to listen for messages at port 2200
            // with the durable transport
            Transports.ListenForMessagesFrom("durable://localhost:2200");


            // Registering a subscription to Message1 that should be
            // delivered to a load balancer Uri at port 2200 and the "important"
            // queue
            Subscribe.To<Message1>().At("durable://loadbalancer:2200/important");

            // Publish the message Message2 to the DNS entry "remoteserver"
            Publish.Message<Message2>().To("durable://remoteserver:2201");
        }
    }
    // ENDSAMPLE


    // SAMPLE: LoopbackTransportApp
    public class LoopbackTransportApp : JasperRegistry
    {
        public LoopbackTransportApp()
        {
            // Configure the maximum number of threads dedicated to processing
            // messages being received by queue. 5 is the default
            Transports.Loopback.DefaultQueue.MaximumParallelization(5);
            Transports.Loopback.Queue("important").MaximumParallelization(10);
            Transports.Loopback.Queue("lowpriority").MaximumParallelization(3);
            Transports.Loopback.Queue("control").Sequential();

            // Publish the message Message2 the important queue
            Publish.Message<Message2>().To("loopback://important");
        }
    }
    // ENDSAMPLE
}
