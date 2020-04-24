using System;
using System.Collections.Generic;
using AdventureGrainInterfaces;
using Orleans.TestingHost;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests
{
    [Collection(ClusterCollection.Name)]
    public class RoomEffects
    {
        private readonly TestCluster _cluster;
        private IRoomGrain room;
        private PlayerInfo playerInfo = new PlayerInfo();

        public RoomEffects(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
            
            //Room Setup
            int num = new Random().Next();
            this.room = _cluster.GrainFactory.GetGrain<IRoomGrain>(num);
            RoomInfo ri = new RoomInfo();
            ri.Description = "This is a test room";
            ri.Directions = new Dictionary<string, long>();
            ri.Id = num;
            ri.Name = "TestRoom";
            this.room.SetInfo(ri).Wait();

            //Player Setup
            this.playerInfo.Key = new Guid();
            this.playerInfo.Name = "TestPlayer";
        }
        
//        public static IEnumerable<object[]> WeatherLists =>
//            new List<object[]>
//            {
//                new object[]{ new List<WeatherTypes>() {WeatherTypes.Blizzard} }, 
//                new object[]{ new List<WeatherTypes>() {WeatherTypes.Cloudy} },
//                new object[]{ new List<WeatherTypes>() {WeatherTypes.Night} },
//                new object[]{ new List<WeatherTypes>() {WeatherTypes.Sunny} }
//            };

        [Fact]
        public async void RoomEffectTest()
        {
            List<WeatherTypes> weathers = new List<WeatherTypes>() {WeatherTypes.Blizzard, WeatherTypes.Cloudy, WeatherTypes.Night, WeatherTypes.Sunny}; // Dark, hailing, cloudy, sunny
            for (int i = 0; i < 1000; i++)
            {
                await this.room.Enter(this.playerInfo);
                string text = await this.room.Description(this.playerInfo);
                Assert.True(text.Contains("hailing") || text.Contains("cloudy") || text.Contains("dark")|| text.Contains("sunny"), text);
            }
        }
    }
}