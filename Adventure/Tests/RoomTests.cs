using System;
using System.Collections.Generic;
using AdventureGrainInterfaces;
using AdventureGrains;
using Moq;
using Orleans.TestingHost;
using Orleans.TestKit;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests
{
    [Collection(ClusterCollection.Name)]
    public class RoomEffects : TestKitBase
    {
        private readonly TestCluster _cluster;
        private Mock<RoomGrain> room;
        private Mock<IRoomGrain> exitRoom;
        //private PlayerInfo playerInfo = new PlayerInfo();

        public RoomEffects(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
            
            //ExitRoom setup
            exitRoom = new Mock<IRoomGrain>();
            
            //Room Setup
            room = new Mock<RoomGrain>();

            room.Setup(x => x.GrainFactory.GetGrain<IRoomGrain>(It.IsAny<long>(),
                "Room")).Returns(exitRoom.Object);
            
            RoomInfo ri = new RoomInfo();
            ri.Description = "Test room with exit to the north";
            ri.Id = 1;
            ri.Name = "Start Room";
            ri.Directions = new Dictionary<string, long>(){{"north", 2}};

            room.Object.SetInfo(ri).Wait();
        }

        [Fact]
        public async void RoomExitTest()
        {
            var exitedTo = await room.Object.ExitTo("north");
            Assert.Equal(exitRoom.Object, exitedTo);
        }

        [Fact]
        public async void RoomDescriptionNoPlayersNoItemsNoMonstersTest()
        {
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "Player"))
                .Returns(player.Object);

            string desc = await room.Object.Description(new PlayerInfo());
            
            Assert.Equal("Your health is: 0\n", desc);
        }
        
        [Fact]
        public async void RoomDescriptionWithPlayersNoItemsNoMonstersTest()
        {
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "Player"))
                .Returns(player.Object);
            
            PlayerInfo pi = new PlayerInfo();
            pi.Name = "testPlayer";
            pi.Key = Guid.NewGuid();
            await room.Object.Enter(pi);
            
            string desc = await room.Object.Description(new PlayerInfo());
            
            Assert.Equal("Beware! These guys are in the room with you:\n  testPlayer\nYour health is: 0\n", desc);
        }

        [Fact]
        public async void RoomPlayerEnterTest()
        {
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "Player"))
                .Returns(player.Object);

            string desc = await room.Object.Enter(new PlayerInfo());
            
            Assert.Equal("Test room with exit to the north\nIt is hailing!\nYour health is: 0\n\n", desc);
        }

        [Fact]
        public async void RoomMonsterEnterMonsterFindTest()
        {
            MonsterInfo mi = new MonsterInfo();
            mi.Name = "testMonster";

            await room.Object.Enter(mi);
            MonsterInfo monster = await room.Object.FindMonster("testMonster");
            
            Assert.Equal(mi, monster);
        }

//        [Fact]
//        public async void RoomEffectTest()
//        {
//            List<WeatherTypes> weathers = new List<WeatherTypes>() {WeatherTypes.Blizzard, WeatherTypes.Cloudy, WeatherTypes.Night, WeatherTypes.Sunny}; // Dark, hailing, cloudy, sunny
//            for (int i = 0; i < 1000; i++)
//            {
//                await this.room.Enter(this.playerInfo);
//                string text = await this.room.Description(this.playerInfo);
//                Assert.True(text.Contains("hailing") || text.Contains("cloudy") || text.Contains("dark")|| text.Contains("sunny"), text);
//            }
//        }
    }
}