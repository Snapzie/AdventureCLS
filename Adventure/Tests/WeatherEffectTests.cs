using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdventureGrainInterfaces;
using AdventureGrains;
using Moq;
using NUnit.Framework;
using Orleans.TestingHost;
using Orleans.TestKit;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests
{
    [Collection(ClusterCollection.Name)]
    public class WeatherEffectTests : TestKitBase
    {
        private readonly TestCluster _cluster;
        private Mock<IPlayerGrain> pg;
        private Mock<IRoomGrain> rg;
        private string desc = "testDesc";

        public WeatherEffectTests(ClusterFixture fixture)
        {
            pg = new Mock<IPlayerGrain>();
            rg = new Mock<IRoomGrain>();
            rg.Setup(_ => _.Description(It.IsAny<PlayerInfo>()))
                .Returns(Task.FromResult("Returned test description"));
        }

        [Fact]
        public async void SunnyWeatherTest()
        {
            SunnyWeather sw = new SunnyWeather();
            
            string res = await sw.WeatherEffect(rg.Object, pg.Object, It.IsAny<PlayerInfo>(), desc);
            
            Assert.Equal("testDesc\nIt is sunny!\nReturned test description\n", res);
        }
        
        [Fact]
        public async void BlizzardWeatherTest()
        {
            BlizzardWeather sw = new BlizzardWeather();
            
            string res = await sw.WeatherEffect(rg.Object, pg.Object, It.IsAny<PlayerInfo>(), desc);
            
            Assert.Equal("testDesc\nIt is hailing!\nReturned test description\n", res);
        }
        
        [Fact]
        public async void CloudyWeatherTest()
        {
            CloudyWeather sw = new CloudyWeather();
            
            string res = await sw.WeatherEffect(rg.Object, pg.Object, It.IsAny<PlayerInfo>(), desc);
            
            Assert.Equal("testDesc\nIt is cloudy!\nReturned test description\n", res);
        }
        
        [Fact]
        public async void NightWeatherTest()
        {
            NightWeather sw = new NightWeather();
            
            string res = await sw.WeatherEffect(rg.Object, pg.Object, It.IsAny<PlayerInfo>(), desc);
            
            Assert.Equal("testDesc\nIt is dark!\nIt is hard to see anything!\n", res);
        }
    }
}