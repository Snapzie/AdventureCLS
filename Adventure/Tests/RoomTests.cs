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
            //Act
            var exitedTo = await room.Object.ExitTo("north");
            //Assert
            Assert.Equal(exitRoom.Object, exitedTo);
        }
        
        [Fact]
        public async void ExitToWithoutRoomTest()
        {
            //Act
            var exitedTo = await room.Object.ExitTo("south");
            //Assert
            Assert.Null(exitedTo);
        }

        [Fact]
        public async void DescriptionNoPlayersNoItemsNoBossNoMonstersTest()
        {
            //Arrange
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);
            //Act
            string desc = await room.Object.Description(new PlayerInfo());
            //Assert
            Assert.Equal("Your health is: 0\n", desc);
        }
        
        [Fact]
        public async void DescriptionWithPlayersNoItemsNoBossNoMonstersTest()
        {
            //Arrange
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);
            PlayerInfo pi = new PlayerInfo();
            pi.Name = "testPlayer";
            pi.Key = Guid.NewGuid();
            await room.Object.Enter(pi);
            //Act
            string desc = await room.Object.Description(new PlayerInfo());
            //Assert
            Assert.Equal("Beware! These guys are in the room with you:\n  testPlayer\nYour health is: 0\n", desc);
        }
        
        [Fact]
        public async void DescriptionNoPlayersNoItemsNoBossWithMonstersTest()
        {
            //Arrange
            var monster = new Mock<IMonsterGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IMonsterGrain>(It.IsAny<Guid>(), "AdventureGrains.Monster"))
                .Returns(monster.Object);
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);
            MonsterInfo mi = new MonsterInfo();
            mi.Name = "testMonster";
            await room.Object.Enter(mi);
            //Act
            string desc = await room.Object.Description(new PlayerInfo());
            //Assert
            Assert.Equal("Beware! These guys are in the room with you:\n  testMonster\nYour health is: 0\n", desc);
        }
        
        [Fact]
        public async void DescriptionNoPlayersNoItemsWithBossNoMonstersTest()
        {
            //Arrange
            var boss = new Mock<IBossGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IBossGrain>(It.IsAny<long>(), "AdventureGrains.Boss"))
                .Returns(boss.Object);
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);
            MonsterInfo bi = new MonsterInfo();
            bi.Name = "testBoss";
            await room.Object.Enter(bi);
            //Act
            string desc = await room.Object.Description(new PlayerInfo());
            //Asserts
            Assert.Equal("Beware! These guys are in the room with you:\n  testBoss\nYour health is: 0\n", desc);
        }
        
        [Fact]
        public async void DescriptionNoPlayersWithItemsNoBossNoMonstersTest()
        {
            //Arrange
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);
            Thing t = new Thing() {Name = "testThing"};
            await this.room.Object.Drop(t);
            //Act
            string desc = await room.Object.Description(new PlayerInfo());
            //Assert
            Assert.Equal("The following things are present:\n  testThing\nYour health is: 0\n", desc);
        }

        [Fact]
        public async void PlayerEnterTest()
        {
            //Arrange
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);
            //Act
            string desc = await room.Object.Enter(new PlayerInfo());
            //Assert
            Assert.Equal("Test room with exit to the north\nIt is hailing!\nYour health is: 0\n\n", desc);
        }
        
        [Fact]
        public async void PlayerExitTest()
        {
            //Arrange
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);
            PlayerInfo pi = new PlayerInfo(){Name = "testPlayer"};
            await this.room.Object.Enter(pi);
            PlayerInfo foundPlayer = await this.room.Object.FindPlayer("testPlayer");
            Assert.Equal(pi, foundPlayer);
            //Act
            await this.room.Object.Exit(pi);
            //Assert
            foundPlayer = await this.room.Object.FindPlayer("testPlayer");

            Assert.Null(foundPlayer);
        }

        [Fact]
        public async void MonsterEnterMonsterFindTest()
        {
            //Arrange
            MonsterInfo mi = new MonsterInfo();
            mi.Name = "testMonster";
            await room.Object.Enter(mi);
            //Act
            MonsterInfo monster = await room.Object.FindMonster("testMonster");
            //Assert
            Assert.Equal(mi, monster);
        }
        
        [Fact]
        public async void MonsterExitTest()
        {
            //Arrange
            MonsterInfo mi = new MonsterInfo(){Name = "testMonster"};
            await this.room.Object.Enter(mi);
            MonsterInfo foundMonster = await this.room.Object.FindMonster("testMonster");
            Assert.Equal(mi, foundMonster);
            //Act
            await this.room.Object.Exit(mi);
            foundMonster = await this.room.Object.FindMonster("testMonster");
            //Assert
            Assert.Null(foundMonster);
        }
        
        [Fact]
        public async void BossEnterGetBossWithBossTest()
        {
            //Arrange
            MonsterInfo bi = new MonsterInfo(){Name = "testBoss"};
            await room.Object.BossEnter(bi);
            //Act
            MonsterInfo boss = await room.Object.GetBoss();
            //Assert
            Assert.Equal(bi, boss);
        }
        
        [Fact]
        public async void GetBossWithoutBossTest()
        {
            //Act
            MonsterInfo boss = await room.Object.GetBoss();
            //Assert
            Assert.Null(boss);
        }
        
        [Fact]
        public async void BossExitTest()
        {
            //Arrange
            MonsterInfo bi = new MonsterInfo(){Name = "testBoss"};
            await room.Object.BossEnter(bi);
            MonsterInfo boss = await room.Object.GetBoss();
            Assert.Equal(bi, boss);
            //Act
            await this.room.Object.BossExit(bi);
            boss = await room.Object.GetBoss();
            //Assert
            Assert.Null(boss);
        }
        
        [Fact]
        public async void MonsterFindNoMonsterTest()
        {
            //Act
            MonsterInfo monster = await room.Object.FindMonster("testMonster");
            //Assert
            Assert.Null(monster);
        }

        [Fact]
        public async void GetTargetsForMonsterWithPlayerTest()
        {
            //Arrange
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);
            PlayerInfo pi = new PlayerInfo() {Key = new Guid(), Name = "testPlayer"};
            await this.room.Object.Enter(pi);
            //Act
            List<PlayerInfo> lpi = await this.room.Object.GetTargetsForMonster();
            //Assert
            Assert.Single(lpi);
            Assert.Equal(pi, lpi[0]);
        }
        
        [Fact]
        public async void GetTargetsForMonsterWithoutPlayerTest()
        {
            //Act
            List<PlayerInfo> lpi = await this.room.Object.GetTargetsForMonster();
            //Assert
            Assert.Empty(lpi);
        }

        [Fact]
        public async void FindPlayerWithPlayerTest()
        {
            //Arrange
            var player = new Mock<IPlayerGrain>();
            room.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player"))
                .Returns(player.Object);
            PlayerInfo pi = new PlayerInfo() {Key = new Guid(), Name = "testPlayer"};
            await this.room.Object.Enter(pi);
            //Act
            PlayerInfo foundPlayer = await this.room.Object.FindPlayer("testPlayer");
            //Assert
            Assert.Equal(pi, foundPlayer);
        }
        
        [Fact]
        public async void FindPlayerWithoutPlayerTest()
        {
            //Act
            PlayerInfo foundPlayer = await this.room.Object.FindPlayer("testPlayer");
            //Assert
            Assert.Null(foundPlayer);
        }

        [Fact]
        public async void DropFindThingTest()
        {
            //Arrange
            Thing t = new Thing() {Name = "testItem"};
            await this.room.Object.Drop(t);
            //Act
            Thing foundThing = await this.room.Object.FindThing("testItem");
            //Assert
            Assert.Equal(t, foundThing);
        }
        
        [Fact]
        public async void TakeSomethingTest()
        {
            //Arrange
            Thing t = new Thing() {Name = "testItem"};
            await this.room.Object.Drop(t);
            Thing foundThing = await this.room.Object.FindThing("testItem");
            Assert.Equal(t, foundThing);
            //Act
            await this.room.Object.Take(t);
            foundThing = await this.room.Object.FindThing("testItem");
            //Assert
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