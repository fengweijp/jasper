﻿using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Consul.Internal;
using Shouldly;
using Xunit;

namespace Jasper.Consul.Testing
{
    public class ConsulBackedRegistrationsTests
    {
        [Fact]
        public void use_the_extension()
        {
            var registry = new JasperRegistry();
            registry.Include<ConsulBackedSubscriptions>();

            using (var runtime = JasperRuntime.For(registry))
            {
                runtime.Get<INodeDiscovery>().ShouldBeOfType<ConsulNodeDiscovery>();
                runtime.Get<ISubscriptionsRepository>()
                    .ShouldBeOfType<ConsulSubscriptionRepository>();
            }
        }
    }

    // SAMPLE: AppUsingConsulBackedSubscriptions
    public class AppUsingConsulBackedSubscriptions : JasperRegistry
    {
        public AppUsingConsulBackedSubscriptions()
        {
            Include<ConsulBackedSubscriptions>();
        }
    }
    // ENDSAMPLE
}
