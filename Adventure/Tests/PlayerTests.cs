using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdventureGrainInterfaces;
using Orleans;
using Orleans.TestingHost;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests
{
    [Collection(ClusterCollection.Name)]
    public class PlayerMonsterInteraction
    {
        private readonly TestCluster _cluster;
        private IRoomGrain room;
        private IPlayerGrain player;
        private IMonsterGrain monster;
        
        public PlayerMonsterInteraction(ClusterFixture fixture)
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
            
            //Monster Setup
            MonsterInfo mi = new MonsterInfo();
            mi.Id = num;
            mi.Name = "TestMonster";
            mi.KilledBy = new List<long>() {};
            this.monster = _cluster.GrainFactory.GetGrain<IMonsterGrain>(mi.Id);
            this.monster.SetInfo(mi).Wait();
            this.monster.SetRoomGrain(this.room).Wait();
            
            //Player Setup
            this.player = _cluster.GrainFactory.GetGrain<IPlayerGrain>(Guid.NewGuid());
            this.player.SetName("TestPlayer").Wait();
            this.player.SetRoomGrain(this.room).Wait();
        }
        
        //Black
        [Fact]
        public async Task FireballDamageTest()
        {
            string text = await this.player.Play("fireball TestMonster");
            Assert.Contains("took 50 damage", text);
        }
        
        [Fact]
        public async Task FireballCooldownTest()
        {
            await this.player.Play("fireball TestMonster");
            string text = await this.player.Play("fireball TestMonster");
            Assert.Equal("Fireball is on cooldown", text);
            
            Thread.Sleep(10001);
            text = await this.player.Play("fireball TestMonster");
            Assert.Equal("TestMonster is dead.", text);
        }

        [Fact]
        public async Task FireballTargetNotFoundTest()
        {
            string text = await this.player.Play("fireball This Is Not A Name");
            Assert.Equal("I can't see This Is Not A Name here. Are you sure?", text);
        }
        
        //White
        [Fact]
        public async Task FireballEmptyStringTest()
        {
            string text = await this.player.Play("fireball ");
            Assert.Equal("Fireball what?", text);
        }
        
        //Blcak
        [Fact]
        public async Task RoarTest()
        {
            Assert.Equal(100, await this.player.GetHealth());
            string text = await this.player.Play("roar");
            Assert.Equal("Roar has been activated!", text);
            Thread.Sleep(2000);
            Assert.Equal(95, await this.player.GetHealth());
            
            Thread.Sleep(10000);
            Assert.Equal(85, await this.player.GetHealth());
        }
        
        [Fact]
        public async Task RoarCooldownTestTest()
        {
            string text = await this.player.Play("roar");
            Assert.Equal("Roar has been activated!", text);
            text = await this.player.Play("roar");
            Assert.Equal("Roar is on cooldown", text);

            Thread.Sleep(20000);
            text = await this.player.Play("roar");
            Assert.Equal("Roar has been activated!", text);
        }
    }
    
    [Collection(ClusterCollection.Name)]
    public class PlayerGrainTests
    {
        private readonly TestCluster _cluster;
        private IRoomGrain room;
        private IPlayerGrain player;
        
        public PlayerGrainTests(ClusterFixture fixture)
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
            this.player = _cluster.GrainFactory.GetGrain<IPlayerGrain>(Guid.NewGuid());
            this.player.SetName("TestPlayer").Wait();
            this.player.SetRoomGrain(this.room).Wait();
        }

        [Fact]
        public async Task DamageTest()
        {
            Assert.Equal(100, await this.player.GetHealth());
            
            await this.player.TakeDamage(this.room, -5);
            Assert.Equal(105, await this.player.GetHealth());
            
            await this.player.TakeDamage(this.room, 50);
            Assert.Equal(55, await this.player.GetHealth());
            
            await this.player.TakeDamage(this.room, 0);
            Assert.Equal(55, await this.player.GetHealth());
        }
        
        [Fact]
        public async Task ItemTests()
        {
            //Make thing
            Thing thing = new Thing();
            thing.Category = "weapon";
            thing.Commands = new List<string>() {"kill"};
            thing.Id = 1;
            thing.Name = "knife";
            thing.FoundIn = this.room.GetPrimaryKeyLong();
            await this.room.Drop(thing);
            //Knife exists in room
            Assert.NotNull(await this.room.FindThing("knife"));
            
            //Player can take knife
            await this.player.Play("take knife");
            Assert.Null(await this.room.FindThing("knife"));
            
            //Player can die
            Assert.Equal(100, await this.player.GetHealth());
            await this.player.TakeDamage(this.room, 200);
            Assert.Equal(-100, await this.player.GetHealth());
            PlayerInfo foundPlayer = await this.room.FindPlayer("TestPlayer");
            Assert.Null(foundPlayer);
            
            //Player drops item on death
            Assert.NotNull(await this.room.FindThing("knife"));
        }
    }
}