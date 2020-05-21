using System;
using AdventureGrains;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using Orleans.TestingHost;
using Xunit;
using Assert = Xunit.Assert;
namespace Tests
{
    [CollectionDefinition(ClusterCollection.Name)]
    public class ClusterCollection : ICollectionFixture<ClusterFixture>
    {
        public const string Name = "ClusterCollection";
    }
    public class ClusterFixture : IDisposable
    {
        public ClusterFixture()
        {
            var testClusterBuilder = new TestClusterBuilder();
            testClusterBuilder.AddSiloBuilderConfigurator<TestSiloBuilderConfigurator>();

            this.Cluster = testClusterBuilder.Build();
            this.Cluster.Deploy();
        }

        public void Dispose()
        {
            this.Cluster.StopAllSilos();
        }

        public TestCluster Cluster { get; private set; }
        
    }
    class TestSiloBuilderConfigurator : ISiloBuilderConfigurator
    {
        public static Action<IServiceCollection> ConfigureServices { get; set; } = services => { };

        public void Configure(ISiloHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureApplicationParts(parts =>
                {
                    parts.AddApplicationPart(typeof(PlayerGrain).Assembly).WithReferences();
                });
        }
    }
}