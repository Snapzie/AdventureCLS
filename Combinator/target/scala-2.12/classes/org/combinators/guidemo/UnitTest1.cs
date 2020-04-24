using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrainInterfaces;
using Grains;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.TestingHost;
using Xunit;

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
                    parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences();
                });
        }
    }
    
    [Collection(ClusterCollection.Name)]
    public class HelloGrainTests
    {
        private readonly TestCluster _cluster;
        
        public HelloGrainTests(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
        }
        
        [Fact]
        public async Task PrimaryKey()
        {
            Guid id = new Guid();
            IHello grain = _cluster.GrainFactory.GetGrain<IHello>(id);
            Guid key = grain.GetPrimaryKey();

            Assert.Equal(id, key);
        }
    }
}