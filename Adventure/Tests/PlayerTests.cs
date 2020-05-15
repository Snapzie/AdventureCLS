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
        
        public PlayerMonsterInteraction(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
            
            MonsterInfo mi = new MonsterInfo();
            mi.Id = 1;
            mi.Name = "testMonster";
            mi.KilledBy = new List<long>();
            
            //Room Setup
            room = new Mock<IRoomGrain>();
            room.Setup(x => x.FindMonster(It.IsAny<string>())).Returns(Task.FromResult(mi));
            
            //Monster Setup
            monster = new Mock<IMonsterGrain>();

            //Player Setup
            player = new Mock<PlayerGrain>();
            player.Setup(x => x.RegisterTimer(It.IsAny<Func<object, Task>>(), 
                    It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
                .Callback<Func<object, Task>, object, TimeSpan, TimeSpan>((a, b, c, d) => { }).Returns(Mock.Of<IDisposable>());
            player.Setup(x => x.GrainFactory.GetGrain<IMonsterGrain>(
                It.IsAny<long>(), "Monster")).Returns(monster.Object);
            player.Object.SetRoomGrain(room.Object).Wait();

//            Func<object, Task> action = null;
//            object state = null;
//            var dueTime = TimeSpan.FromSeconds(2);
//            var period = TimeSpan.FromSeconds(1);
//            player.Setup(x => x.RegisterTimer(It.IsAny<Func<object, Task>>(), 
//                    It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
//                .Callback<Func<object, Task>, object, TimeSpan, TimeSpan>((a, b, c, d) =>
//                {
//                    action = a;
//                    state = b;
//                    dueTime = c;
//                    period = d;
//                }).Returns(Mock.Of<IDisposable>());
        }
        
        public async void Dispose()
        {
            //Necessary to dispose timers
            //await this.monster.Kill(this.room, 999);
        }
        
        [Fact]
        public async void FireballTestMonster()
        {
            monster.Setup(x => x.Kill(It.IsAny<IRoomGrain>(), It.IsAny<int>()))
                .Returns(Task.FromResult("testMonster took 50 damage. He now has 50 health left!"));

            room.Setup(x => x.FindPlayer(It.IsAny<string>())).Returns(Task.FromResult<PlayerInfo>(null));

            string res = await player.Object.Play("fireball testMonster");

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
                "Player")).Returns(enemyPlayer.Object);
            
            room.Setup(x => x.FindPlayer(It.IsAny<string>())).Returns(Task.FromResult<PlayerInfo>(pi));

            enemyPlayer.Setup(x => x.TakeDamage(It.IsAny<IRoomGrain>(), It.IsAny<int>()))
                .Returns(Task.FromResult("testPlayer took 50 damage and now has 0 health left!"));

            string res = await player.Object.Play("fireball testPlayer");

            Assert.Equal("testPlayer took 50 damage and now has 0 health left!", res);
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