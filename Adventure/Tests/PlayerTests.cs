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
    public class PlayerMonsterInteraction : TestKitBase, IDisposable
    {
        private readonly TestCluster _cluster;
        private Mock<IRoomGrain> room;
        private Mock<PlayerGrain> player;
        private Mock<IMonsterGrain> monster;
        MonsterInfo mi;
        RoomInfo ri;
        Guid roomGUID;


        public PlayerMonsterInteraction(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
            
            mi = new MonsterInfo();
            mi.Id = 1;
            mi.Name = "testMonster";
            mi.KilledBy = new List<long>();
            
            //Room Setup
            room = new Mock<IRoomGrain>();
            ri = new RoomInfo();
            ri.Id = 1;
            roomGUID = new Guid();
            
            //Monster Setup
            monster = new Mock<IMonsterGrain>();

            //Player Setup
            player = new Mock<PlayerGrain>();
            //player.Setup(x => x.RegisterTimer(It.IsAny<Func<object, Task>>(), 
            //        It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
            //    .Callback<Func<object, Task>, object, TimeSpan, TimeSpan>((a, b, c, d) => { }).Returns(Mock.Of<IDisposable>());
            player.Setup(x => x.GrainFactory.GetGrain<IMonsterGrain>(
                It.IsAny<long>(), "Monster")).Returns(monster.Object);
            player.Object.SetRoomGrain(room.Object).Wait();

        //    Func<object, Task> action = null;
        //    object state = null;
        //    var dueTime = TimeSpan.FromSeconds(2);
        //    var period = TimeSpan.FromSeconds(1);
        //    player.Setup(x => x.RegisterTimer(It.IsAny<Func<object, Task>>(),
        //            It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
        //        .Callback<Func<object, Task>, object, TimeSpan, TimeSpan>((a, b, c, d) =>
        //        {
        //            action = a;
        //            state = b;
        //            dueTime = c;
        //            period = d;
        //        }).Returns(Mock.Of<IDisposable>());
        }
        
        public async void Dispose()
        {
            //Necessary to dispose timers
            //await this.monster.Kill(this.room, 999);
        }

        [Fact]
        public async void StartHealthTest()
        {
            Assert.Equal(100, await player.Object.GetHealth());
        }
        
        [Fact]
        public async void FireballTestMonster()
        {
            //Arrange
            monster.Setup(x => x.Kill(It.IsAny<IRoomGrain>(), It.IsAny<int>()))
                .Returns(Task.FromResult("testMonster took 50 damage. He now has 50 health left!"));

            player.Setup(p => p.GrainFactory.GetGrain<IMonsterGrain>(It.IsAny<long>(), "AdventureGrains.Monster")).Returns(monster.Object);
            room.Setup(x => x.FindMonster(It.IsAny<string>())).Returns(Task.FromResult(mi));

            //Act
            string res = await player.Object.Play("fireball testMonster");
            //Assert
            Assert.Equal("testMonster took 50 damage. He now has 50 health left!", res);
        }
        
        [Fact]
        public async void FireballTestPlayer()
        {
            var enemyPlayer = new Mock<IPlayerGrain>();
            PlayerInfo pi = new PlayerInfo();
            pi.Key = new Guid();
            pi.Name = "testPlayer";

            player.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(),
                "AdventureGrains.Player")).Returns(enemyPlayer.Object);
            
            room.Setup(x => x.FindPlayer(It.IsAny<string>())).Returns(Task.FromResult<PlayerInfo>(pi));

            enemyPlayer.Setup(x => x.TakeDamage(It.IsAny<IRoomGrain>(), It.IsAny<int>()))
                .Returns(Task.FromResult("testPlayer took 50 damage and now has 0 health left!"));

            string res = await player.Object.Play("fireball testPlayer");

            Assert.Equal("testPlayer took 50 damage and now has 0 health left!", res);
        }

        [Fact]
        public async void FireballTestNoOne() //REDUNDANCY I FIREBALL CASE? Eller empty
        {
            //Arrange done in Constructor

            //Arrange
            string res = await player.Object.Play("fireball");
            //Assert
            Assert.Equal("Fireball what?", res);

            //Arrange
            string res2 = await player.Object.Play("fireball No One");
            //Assert
            Assert.Equal("I can't see No One here. Are you sure?", res2);
        }

        [Fact]
        public async void FireballTestCooldown() //Fireball does not come off cooldown
        {
            //Arrange
            Func<object, Task> action = null;
            object state = null;
            TimeSpan dueTime = TimeSpan.FromSeconds(100);
            TimeSpan period = TimeSpan.FromSeconds(100);
            player.Setup(x => x.RegisterTimer(It.IsAny<Func<object, Task>>(),
                    It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
                .Callback<Func<object, Task>, object, TimeSpan, TimeSpan>((a, b, c, d) =>
                {
                    action = a;
                    state = b;
                    dueTime = c;
                    period = d;
                }).Returns(Mock.Of<IDisposable>());
            player.Setup(p => p.GrainFactory.GetGrain<IMonsterGrain>(It.IsAny<long>(), "AdventureGrains.Monster")).Returns(monster.Object);
            room.Setup(r => r.FindMonster(It.IsAny<string>())).Returns(Task.FromResult(mi));
            await player.Object.Play("Fireball testMonster");

            //Act
            string res = await player.Object.Play("Fireball testMonster");

            //Assert
            Assert.Equal("Fireball is on cooldown", res);
            Assert.NotNull(action);
            Assert.Equal(10, dueTime.TotalSeconds);
            Assert.Equal(-1, period.TotalSeconds);
            Assert.Null(state);
        }

        [Fact]
        public async void RoarTest()
        {
            //Arrange
            //room.Setup(r => r.GetId()).Returns(Task.FromResult(ri.Id));

            //Act
            string res = await this.player.Object.Play("roar someone");
            //Assert
            Assert.Equal("Can not roar others", res);

            //Act
            string res2 = await this.player.Object.Play("roar");

            //Assert
            Assert.Equal("Roar has been activated!", res2);

            //Act
            await this.player.Object.TakeDamage(this.room.Object, 50);
            //Assert
            Assert.Equal(75, await this.player.Object.GetHealth());

            //Act
            await this.player.Object.TakeDamage(this.room.Object, 0);
            //Assert
            Assert.Equal(75, await this.player.Object.GetHealth());

        }

        [Fact]
        public async void RoarCooldownTest() //TakeDamage prob
        {
            //Arrange
            Func<object, Task> action = null;
            object state = null;
            TimeSpan dueTime = TimeSpan.FromSeconds(100);
            TimeSpan period = TimeSpan.FromSeconds(100);
            player.Setup(x => x.RegisterTimer(It.IsAny<Func<object, Task>>(),
                    It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
                .Callback<Func<object, Task>, object, TimeSpan, TimeSpan>((a, b, c, d) =>
                {
                    action = a;
                    state = b;
                    dueTime = c;
                    period = d;
                }).Returns(Mock.Of<IDisposable>());
            await this.player.Object.Play("roar");

            //Act
            string res = await this.player.Object.Play("roar");
            //Assert
            Assert.Equal("Roar is on cooldown", res);
            Assert.NotNull(action);
            Assert.Equal(20, dueTime.TotalSeconds);
            Assert.Equal(-1, period.TotalSeconds);
            Assert.Null(state);
        }

        [Fact]
        public async void DieTest() // :THONKING:
        {
            //Arrange
            var somePlayer = new Mock<IPlayerGrain>();
            PlayerInfo pi = new PlayerInfo();
            pi.Key = new Guid();
            pi.Name = "testPlayer";
            room.Setup(x => x.FindPlayer(It.IsAny<string>())).Returns(Task.FromResult<PlayerInfo>(pi));

        }

        [Fact]
        public async void TakeDamageTest() //GetPrimaryKey is inaccessible and non-overridable
        {
            //Arrange partly done in Constructor
            //room.Setup(r => r.GetId()).Returns(Task.FromResult(ri.Id));
            PlayerInfo pi = new PlayerInfo();
            pi.Key = new Guid();
            player.Setup(p => p.GetId()).Returns(Task.FromResult(pi.Key));
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

            //Act
            await this.player.Object.TakeDamage(this.room.Object, int.MinValue); //Player dies, myinfo is NULL FAM WOWIE
            //Assert
            Assert.Equal(-2147483544, await this.player.Object.GetHealth());

        }

        [Fact]
        public async void GoRoomTest()
        {

        }

        [Fact]
        public async void DropTest()
        {

        }

        [Fact]
        public async void TakeTest()
        {

        }

        //Black
        //        [Fact]
        //        public async Task FireballDamageTest()
        //        {
        //            string text = await this.player.Play("fireball TestMonster");
        //            Assert.Contains("took 50 damage", text);
        //        }
        //        
        //        [Fact]
        //        public async Task FireballCooldownTest()
        //        {
        //            await this.player.Play("fireball TestMonster");
        //            string text = await this.player.Play("fireball TestMonster");
        //            Assert.Equal("Fireball is on cooldown", text);
        //            
        //            Thread.Sleep(10010);
        //            text = await this.player.Play("fireball TestMonster");
        //            Assert.Equal("TestMonster is dead.", text);
        //        }
        //
        //        [Fact]
        //        public async Task FireballTargetNotFoundTest()
        //        {
        //            string text = await this.player.Play("fireball This Is Not A Name");
        //            Assert.Equal("I can't see This Is Not A Name here. Are you sure?", text);
        //        }
        //        
        //        //White
        //        [Fact]
        //        public async Task FireballEmptyStringTest()
        //        {
        //            string text = await this.player.Play("fireball ");
        //            Assert.Equal("Fireball what?", text);
        //        }
        //        
        //        //Blcak
        //        [Fact]
        //        public async Task RoarTest()
        //        {
        //            Assert.Equal(100, await this.player.GetHealth());
        //            string text = await this.player.Play("roar");
        //            Assert.Equal("Roar has been activated!", text);
        //            Thread.Sleep(2010);
        //            Assert.Equal(95, await this.player.GetHealth());
        //            
        //            Thread.Sleep(10010);
        //            Assert.Equal(85, await this.player.GetHealth());
        //        }
        //        
        //        [Fact]
        //        public async Task RoarCooldownTest()
        //        {
        //            string text = await this.player.Play("roar");
        //            Assert.Equal("Roar has been activated!", text);
        //            text = await this.player.Play("roar");
        //            Assert.Equal("Roar is on cooldown", text);
        //
        //            Thread.Sleep(20010);
        //            text = await this.player.Play("roar");
        //            Assert.Equal("Roar has been activated!", text);
        //        }
        //    }
        //    
        //    [Collection(ClusterCollection.Name)]
        //    public class PlayerGrainTests
        //    {
        //        private readonly TestCluster _cluster;
        //        private IRoomGrain room;
        //        private IPlayerGrain player;
        //        
        //        public PlayerGrainTests(ClusterFixture fixture)
        //        {
        //            _cluster = fixture.Cluster;
        //            
        //            //Room Setup
        //            int num = new Random().Next();
        //            this.room = _cluster.GrainFactory.GetGrain<IRoomGrain>(num);
        //            RoomInfo ri = new RoomInfo();
        //            ri.Description = "This is a test room";
        //            ri.Directions = new Dictionary<string, long>();
        //            ri.Id = num;
        //            ri.Name = "TestRoom";
        //            this.room.SetInfo(ri).Wait();
        //            
        //            //Player Setup
        //            this.player = _cluster.GrainFactory.GetGrain<IPlayerGrain>(Guid.NewGuid());
        //            this.player.SetName("TestPlayer").Wait();
        //            this.player.SetRoomGrain(this.room).Wait();
        //        }
        //
        //        [Fact]
        //        public async Task DamageTest()
        //        {
        //            Assert.Equal(100, await this.player.GetHealth());
        //            
        //            await this.player.TakeDamage(this.room, -5);
        //            Assert.Equal(105, await this.player.GetHealth());
        //            
        //            await this.player.TakeDamage(this.room, 50);
        //            Assert.Equal(55, await this.player.GetHealth());
        //            
        //            await this.player.TakeDamage(this.room, 0);
        //            Assert.Equal(55, await this.player.GetHealth());
        //        }
        //        
        //        [Fact]
        //        public async Task ItemTests()
        //        {
        //            //Make thing
        //            Thing thing = new Thing();
        //            thing.Category = "weapon";
        //            thing.Commands = new List<string>() {"kill"};
        //            thing.Id = 1;
        //            thing.Name = "knife";
        //            thing.FoundIn = this.room.GetPrimaryKeyLong();
        //            await this.room.Drop(thing);
        //            //Knife exists in room
        //            Assert.NotNull(await this.room.FindThing("knife"));
        //            
        //            //Player can take knife
        //            await this.player.Play("take knife");
        //            Assert.Null(await this.room.FindThing("knife"));
        //            
        //            //Player can die
        //            Assert.Equal(100, await this.player.GetHealth());
        //            await this.player.TakeDamage(this.room, 200);
        //            Assert.Equal(-100, await this.player.GetHealth());
        //            PlayerInfo foundPlayer = await this.room.FindPlayer("TestPlayer");
        //            Assert.Null(foundPlayer);
        //            
        //            //Player drops item on death
        //            Assert.NotNull(await this.room.FindThing("knife"));
        //        }
    }
}