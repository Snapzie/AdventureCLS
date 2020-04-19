using System;
using System.Collections.Generic;
using System.Threading;
using AdventureGrainInterfaces;
using AdventureGrains;
using Orleans.TestingHost;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests
{
    [Collection(ClusterCollection.Name)]
    public class BossTests
    {
        private readonly TestCluster _cluster;
        private IRoomGrain room;
        private PlayerInfo playerInfo = new PlayerInfo();
        private IBossGrain boss;

        public BossTests(ClusterFixture fixture)
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

            //Boss Setup
            this.boss = _cluster.GrainFactory.GetGrain<IBossGrain>(num);
            this.boss.SetInfo().Wait(); 
            this.boss.SetRoomGrain(this.room).Wait();
        }
    

        [Fact]
        public async void AddSpawnTest()
        {
            await this.room.Enter(this.playerInfo);
            Assert.NotNull(this.room.GetBoss());
            Assert.Empty(await this.room.GetMonsters());
            
            Thread.Sleep(5001);
            Assert.Single(this.room.GetMonsters().Result);
            
            //Necessary to dispose timers
            long id = this.room.GetBoss().Result.Id;
            IBossGrain monster = _cluster.GrainFactory.GetGrain<IBossGrain>(id);
            await monster.Kill(this.room, 999);
        }
        
        [Fact]
        public async void AddHealTest()
        {
            await this.room.Enter(this.playerInfo);
            Assert.NotNull(this.room.GetBoss());
            Assert.Empty(await this.room.GetMonsters());
            
            Thread.Sleep(5001);
            Assert.Single(this.room.GetMonsters().Result);
            long id = this.room.GetMonsters().Result[0].Id;
            IMonsterGrain monster = _cluster.GrainFactory.GetGrain<IMonsterGrain>(id);
            string text = await monster.Kill(this.room, 0);
            Assert.Contains("100 health left!", text);
            
            Thread.Sleep(5000);
            text = await monster.Kill(this.room, 0);
            Assert.Contains("110 health left!", text);
            
            //Necessary to dispose timers
            id = this.room.GetBoss().Result.Id;
            IBossGrain boss = _cluster.GrainFactory.GetGrain<IBossGrain>(id);
            await boss.Kill(this.room, 999);
        }
    }
}