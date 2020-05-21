using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdventureGrainInterfaces;
using AdventureGrains;
using Moq;
using Orleans;
using Orleans.TestingHost;
using Orleans.TestKit;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests
{
    [Collection(ClusterCollection.Name)]
    public class PlayerTests : TestKitBase
    {
        private readonly TestCluster _cluster;
        private Mock<IRoomGrain> room;
        private Mock<PlayerGrain> player;
        private Mock<IMonsterGrain> monster;
        MonsterInfo mi;
        RoomInfo ri;


        public PlayerTests(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
            
            mi = new MonsterInfo();
            mi.Id = 1;
            mi.Name = "testMonster";
            mi.KilledBy = new List<long>{ 0 };
            
            //Room Setup
            room = new Mock<IRoomGrain>();
            ri = new RoomInfo();
            ri.Id = 1;
            
            //Monster Setup
            monster = new Mock<IMonsterGrain>();

            //Player Setup
            player = new Mock<PlayerGrain>();
            player.Setup(x => x.GrainFactory.GetGrain<IMonsterGrain>(
                It.IsAny<long>(), "Monster")).Returns(monster.Object);
            player.Object.SetRoomGrain(room.Object).Wait();
        }

        [Fact]
        public async void StartHealthTest()
        {
            Assert.Equal(100, await player.Object.GetHealth());
        }

        [Fact]
        public async void TakeDamageTest() //GetPrimaryKey is inaccessible and non-overridable
        {
            //Arrange partly done in Constructor
            PlayerInfo pi = new PlayerInfo();
            pi.Key = new Guid();
            player.Setup(p => p.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player")).Returns(player.Object);

            //Act
            await this.player.Object.TakeDamage(this.room.Object, -5);
            //Assert
            Assert.Equal(105, await this.player.Object.GetHealth());

            //Act
            await this.player.Object.TakeDamage(this.room.Object, 1);
            //Assert
            Assert.Equal(104, await this.player.Object.GetHealth());

            //Act
            await this.player.Object.TakeDamage(this.room.Object, 0);
            //Assert
            Assert.Equal(104, await this.player.Object.GetHealth());
        }

        [Fact]
        public async void GoRoomTest()
        {
            //Arrange
            Mock<IRoomGrain> newRoom = new Mock<IRoomGrain>();
            newRoom.Setup(nr => nr.Enter(It.IsAny<PlayerInfo>())).Returns(Task.FromResult("Some description"));
            room.Setup(r => r.ExitTo(It.IsAny<string>())).Returns(Task.FromResult(newRoom.Object));

            //Act
            string res = await this.player.Object.Play("north");

            //Assert
            Assert.Equal("Some description", res);
        }

        [Fact]
        public async void GoNoAdjacentRoomTest()
        {
            //Act
            string res = await this.player.Object.Play("north");

            //Assert
            Assert.Equal("You cannot go in that direction.", res);
        }

        [Fact]
        public async void TakeTest()
        {
            //Arrange
            Thing knife = new Thing();
            knife.Name = "knife";
            room.Setup(r => r.FindThing(It.IsAny<string>())).Returns(Task.FromResult(knife));

            //Act
            string res = await this.player.Object.Play("take knife");

            //Assert
            Assert.Equal("Okay.", res);
        }

        [Fact]
        public async void TakeNoItemTest()
        {
            //Act
            string res = await this.player.Object.Play("take knife");

            //Assert
            Assert.Equal("I don't understand.", res);
        }

        [Fact]
        public async void TakeNoInputTest()
        {
            //Act
            string res = await this.player.Object.Play("take");

            //Assert
            Assert.Equal("I don't understand.", res);
        }

        [Fact]
        public async void DropTest()
        {
            //Arrange
            Thing knife = new Thing();
            knife.Name = "knife";
            room.Setup(r => r.FindThing(It.IsAny<string>())).Returns(Task.FromResult(knife));
            room.Setup(r => r.Take(It.IsAny<Thing>())).Returns(Task.FromResult(knife));
            await this.player.Object.Play("take knife");

            //Act
            string res = await this.player.Object.Play("Drop knife");

            //Assert
            Assert.Equal("Okay.", res);
        }

        [Fact]
        public async void DropNoItemTest()
        {
            //Act
            string res = await this.player.Object.Play("Drop knife");

            //Assert
            Assert.Equal("I don't understand.", res);
        }

        [Fact]
        public async void DropNoInputTest()
        {
            //Act
            string res = await this.player.Object.Play("Drop");

            //Assert
            Assert.Equal("I don't understand.", res);
        }

        [Fact]
        public async void KillPlayerTest()
        {
            //Arrange
            Thing knife = new Thing();
            knife.Name = "knife";
            knife.Category = "weapon";
            room.Setup(r => r.FindThing(It.IsAny<string>())).Returns(Task.FromResult(knife));
            room.Setup(r => r.Take(It.IsAny<Thing>())).Returns(Task.FromResult(knife));
            await this.player.Object.Play("take knife");
            Mock<IPlayerGrain> enemyPlayer = new Mock<IPlayerGrain>();
            PlayerInfo pi = new PlayerInfo();
            pi.Key = new Guid();
            pi.Name = "testPlayer";
            room.Setup(r => r.FindPlayer(It.IsAny<string>())).Returns(Task.FromResult(pi));
            player.Setup(p => p.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(), "AdventureGrains.Player")).Returns(enemyPlayer.Object);

            //Act
            string res = await player.Object.Play("kill testPlayer");

            //Assert
            Assert.Equal("testPlayer is now dead.", res);

        }

        [Fact]
        public async void KillMonsterTest()
        {
            //Arrange
            monster.Setup(m => m.Kill(It.IsAny<IRoomGrain>(), It.IsAny<int>())).Returns(Task.FromResult("Oof!"));
            player.Setup(p => p.GrainFactory.GetGrain<IMonsterGrain>(It.IsAny<long>(), "AdventureGrains.Monster")).Returns(monster.Object);
            Thing knife = new Thing();
            knife.Name = "knife";
            knife.Id = 0;
            room.Setup(r => r.FindThing(It.IsAny<string>())).Returns(Task.FromResult(knife));
            room.Setup(r => r.Take(It.IsAny<Thing>())).Returns(Task.FromResult(knife));
            room.Setup(r => r.FindMonster(It.IsAny<string>())).Returns(Task.FromResult(mi));
            await this.player.Object.Play("take knife");

            //Act
            string res = await this.player.Object.Play("kill testMonster");

            //Assert
            Assert.Equal("Oof!", res);
        }

        [Fact]
        public async void KillNoTargetTest()
        {
            //Act
            string res = await this.player.Object.Play("kill No One");

            //Assert
            Assert.Equal("With what? Your bare hands?", res);
        }

        [Fact]
        public async void KillNoTargetWithItemTest()
        {
            //Arrange
            Thing knife = new Thing();
            knife.Name = "knife";
            room.Setup(r => r.FindThing(It.IsAny<string>())).Returns(Task.FromResult(knife));
            room.Setup(r => r.Take(It.IsAny<Thing>())).Returns(Task.FromResult(knife));
            await this.player.Object.Play("take knife");

            //Act
            string res = await this.player.Object.Play("kill No One");

            //Assert
            Assert.Equal("I can't see No One here. Are you sure?", res);
        }

        [Fact]
        public async void KillNoOneTest()
        {
            //Act
            string res = await this.player.Object.Play("kill");

            //Assert
            Assert.Equal("Kill what?", res);
        }

        [Fact]
        public async void LookTest()
        {
            //Arrange
            room.Setup(r => r.Description(It.IsAny<PlayerInfo>())).Returns(Task.FromResult("This is a room."));

            //Act
            string res = await this.player.Object.Play("look");

            //Assert
            Assert.Equal("This is a room.", res);
        }

        [Fact]
        public async void InventoryTest()
        {
            //Arrange
            Thing knife = new Thing();
            knife.Name = "knife";
            room.Setup(r => r.FindThing(It.IsAny<string>())).Returns(Task.FromResult(knife));
            room.Setup(r => r.Take(It.IsAny<Thing>())).Returns(Task.FromResult(knife));
            await this.player.Object.Play("take knife");

            //Act
            string res = await this.player.Object.Play("inv");

            //Assert
            Assert.Contains("knife", res);

            //Act
            string res2 = await this.player.Object.Play("inventory");

            //Assert
            Assert.Contains("knife", res2);
        }

        [Fact]
        public async void InventoryEmptyTest()
        {
            //Act
            string res = await this.player.Object.Play("inv");
            //Assert
            Assert.Equal("You are carrying: ", res);

            //Act
            string res2 = await this.player.Object.Play("inventory");
            //Assert
            Assert.Equal("You are carrying: ", res2);
        }

        [Fact]
        public async void EndTest()
        {
            //Act
            string res = await this.player.Object.Play("end");

            //Assert
            Assert.Equal("", res);
        }

        [Fact]
        public async void NoInputTest()
        {
            //Act
            string res = await this.player.Object.Play("");

            //Assert
            Assert.Equal("I don't understand.", res);
        }

        [Fact]
        public async void InvalidInputTest()
        {
            //Act
            string res = await this.player.Object.Play("123#?!_hey");

            //Assert
            Assert.Equal("I don't understand.", res);
        }
    }
}