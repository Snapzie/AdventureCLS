using System;
using System.Collections.Generic;
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
    public class RoomTests : TestKitBase
    {
        private readonly TestCluster _cluster;
        private Mock<RoomGrain> room;
        private Mock<IRoomGrain> exitRoom;
        //private PlayerInfo playerInfo = new PlayerInfo();

        public RoomTests(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
            
            //ExitRoom setup
            exitRoom = new Mock<IRoomGrain>();
            
            //Room Setup
            room = new Mock<RoomGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IRoomGrain>(It.IsAny<long>(),
                "AdventureGrains.Room")).Returns(exitRoom.Object);
            
            RoomInfo ri = new RoomInfo();
            ri.Description = "Test room with exit to the north";
            ri.Id = 1;
            ri.Name = "Start Room";
            ri.Directions = new Dictionary<string, long>(){{"north", 2}};

            room.Object.SetInfo(ri).Wait();
        }

        [Fact]
        public async void ExitToWithRoomTest()
        {
            var exitedTo = await room.Object.ExitTo("north");
            Assert.Equal(exitRoom.Object, exitedTo);
        }
        
        [Fact]
        public async void ExitToWithoutRoomTest()
        {
            var exitedTo = await room.Object.ExitTo("south");
            Assert.Null(exitedTo);
        }

        [Fact]
        public async void DescriptionNoPlayersNoItemsNoBossNoMonstersTest()
        {
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);

            string desc = await room.Object.Description(new PlayerInfo());
            
            Assert.Equal("Your health is: 0\n", desc);
        }
        
        [Fact]
        public async void DescriptionWithPlayersNoItemsNoBossNoMonstersTest()
        {
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);
            
            PlayerInfo pi = new PlayerInfo();
            pi.Name = "testPlayer";
            pi.Key = Guid.NewGuid();
            await room.Object.Enter(pi);
            
            string desc = await room.Object.Description(new PlayerInfo());
            
            Assert.Equal("Beware! These guys are in the room with you:\n  testPlayer\nYour health is: 0\n", desc);
        }
        
        [Fact]
        public async void DescriptionNoPlayersNoItemsNoBossWithMonstersTest()
        {
            var monster = new Mock<IMonsterGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IMonsterGrain>(It.IsAny<Guid>(), "AdventureGrains.Monster"))
                .Returns(monster.Object);
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);
            
            MonsterInfo mi = new MonsterInfo();
            mi.Name = "testMonster";
            await room.Object.Enter(mi);

            string desc = await room.Object.Description(new PlayerInfo());
            
            Assert.Equal("Beware! These guys are in the room with you:\n  testMonster\nYour health is: 0\n", desc);
        }
        
        [Fact]
        public async void DescriptionNoPlayersNoItemsWithBossNoMonstersTest()
        {
            var boss = new Mock<IBossGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IBossGrain>(It.IsAny<long>(), "AdventureGrains.Boss"))
                .Returns(boss.Object);
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);
            
            MonsterInfo bi = new MonsterInfo();
            bi.Name = "testBoss";
            await room.Object.Enter(bi);

            string desc = await room.Object.Description(new PlayerInfo());
            
            Assert.Equal("Beware! These guys are in the room with you:\n  testBoss\nYour health is: 0\n", desc);
        }
        
        [Fact]
        public async void DescriptionNoPlayersWithItemsNoBossNoMonstersTest()
        {
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);
            
            Thing t = new Thing() {Name = "testThing"};
            await this.room.Object.Drop(t);

            string desc = await room.Object.Description(new PlayerInfo());
            
            Assert.Equal("The following things are present:\n  testThing\nYour health is: 0\n", desc);
        }

        [Fact]
        public async void PlayerEnterTest()
        {
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);

            string desc = await room.Object.Enter(new PlayerInfo());
            
            Assert.Equal("Test room with exit to the north\nIt is hailing!\nYour health is: 0\n\n", desc);
        }
        
        [Fact]
        public async void PlayerExitTest()
        {
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);
            PlayerInfo pi = new PlayerInfo(){Name = "testPlayer"};
            
            await this.room.Object.Enter(pi);
            PlayerInfo foundPlayer = await this.room.Object.FindPlayer("testPlayer");
            Assert.Equal(pi, foundPlayer);
            await this.room.Object.Exit(pi);
            foundPlayer = await this.room.Object.FindPlayer("testPlayer");

            Assert.Null(foundPlayer);
        }

        [Fact]
        public async void MonsterEnterMonsterFindTest()
        {
            MonsterInfo mi = new MonsterInfo();
            mi.Name = "testMonster";

            await room.Object.Enter(mi);
            MonsterInfo monster = await room.Object.FindMonster("testMonster");
            
            Assert.Equal(mi, monster);
        }
        
        [Fact]
        public async void MonsterExitTest()
        {
            MonsterInfo mi = new MonsterInfo(){Name = "testMonster"};
            
            await this.room.Object.Enter(mi);
            MonsterInfo foundMonster = await this.room.Object.FindMonster("testMonster");
            Assert.Equal(mi, foundMonster);
            await this.room.Object.Exit(mi);
            foundMonster = await this.room.Object.FindMonster("testMonster");
            
            Assert.Null(foundMonster);
        }
        
        [Fact]
        public async void BossEnterGetBossWithBossTest()
        {
            MonsterInfo bi = new MonsterInfo(){Name = "testBoss"};

            await room.Object.BossEnter(bi);
            MonsterInfo boss = await room.Object.GetBoss();
            
            Assert.Equal(bi, boss);
        }
        
        [Fact]
        public async void GetBossWithoutBossTest()
        {
            MonsterInfo boss = await room.Object.GetBoss();
            
            Assert.Null(boss);
        }
        
        [Fact]
        public async void BossExitTest()
        {
            MonsterInfo bi = new MonsterInfo(){Name = "testBoss"};

            await room.Object.BossEnter(bi);
            MonsterInfo boss = await room.Object.GetBoss();
            Assert.Equal(bi, boss);
            await this.room.Object.BossExit(bi);
            boss = await room.Object.GetBoss();
            
            Assert.Null(boss);
        }
        
        [Fact]
        public async void MonsterFindNoMonsterTest()
        {
            MonsterInfo monster = await room.Object.FindMonster("testMonster");
            Assert.Null(monster);
        }

        [Fact]
        public async void GetTargetsForMonsterWithPlayerTest()
        {
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);
            PlayerInfo pi = new PlayerInfo() {Key = new Guid(), Name = "testPlayer"};
            await this.room.Object.Enter(pi);

            List<PlayerInfo> lpi = await this.room.Object.GetTargetsForMonster();

            Assert.Single(lpi);
            Assert.Equal(pi, lpi[0]);
        }
        
        [Fact]
        public async void GetTargetsForMonsterWithoutPlayerTest()
        {
            List<PlayerInfo> lpi = await this.room.Object.GetTargetsForMonster();

            Assert.Empty(lpi);
        }

        [Fact]
        public async void FindPlayerWithPlayerTest()
        {
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);
            PlayerInfo pi = new PlayerInfo() {Key = new Guid(), Name = "testPlayer"};
            await this.room.Object.Enter(pi);

            PlayerInfo foundPlayer = await this.room.Object.FindPlayer("testPlayer");
            
            Assert.Equal(pi, foundPlayer);
        }
        
        [Fact]
        public async void FindPlayerWithoutPlayerTest()
        {
            PlayerInfo foundPlayer = await this.room.Object.FindPlayer("testPlayer");
            
            Assert.Null(foundPlayer);
        }

        [Fact]
        public async void DropFindThingTest()
        {
            Thing t = new Thing() {Name = "testItem"};

            await this.room.Object.Drop(t);
            Thing foundThing = await this.room.Object.FindThing("testItem");
            
            Assert.Equal(t, foundThing);
        }
        
        [Fact]
        public async void TakeSomethingTest()
        {
            Thing t = new Thing() {Name = "testItem"};

            await this.room.Object.Drop(t);
            Thing foundThing = await this.room.Object.FindThing("testItem");
            Assert.Equal(t, foundThing);
            await this.room.Object.Take(t);
            foundThing = await this.room.Object.FindThing("testItem");
            
            Assert.Null(foundThing);
        }
        
        [Fact]
        public async void TakeNothingTest()
        {
            await this.room.Object.Take(It.IsAny<Thing>());
            Thing foundThing = await this.room.Object.FindThing("testItem");
            
            Assert.Null(foundThing);
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