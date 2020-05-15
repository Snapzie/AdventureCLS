using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdventureGrainInterfaces;
using AdventureGrains;
using Microsoft.VisualBasic.CompilerServices;
using Orleans.TestingHost;
using Xunit;
using Assert = Xunit.Assert;
using Moq;
using Orleans;
using Orleans.Runtime;
using Orleans.TestKit;

namespace Tests
{
    [Collection(ClusterCollection.Name)]
    public class BossTests : TestKitBase
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

        public async void Dispose()
        {
            //Necessary to dispose timers
            await this.boss.Kill(this.room, 999);
        }

//        [Fact]
//        public async void AddSpawnTest()
//        {
//            await this.room.Enter(this.playerInfo);
//            Assert.NotNull(this.room.GetBoss());
//            Assert.Empty(await this.room.GetMonsters());
//            
//            Thread.Sleep(5010);
//            Assert.Single(this.room.GetMonsters().Result);
//        }
//        
//        [Fact]
//        public async void AddHealTest()
//        {
//            await this.room.Enter(this.playerInfo);
//            Assert.NotNull(this.room.GetBoss());
//            Assert.Empty(await this.room.GetMonsters());
//            
//            Thread.Sleep(5010);
//            Assert.Single(this.room.GetMonsters().Result);
//            long id = this.room.GetMonsters().Result[0].Id;
//            IMonsterGrain monster = _cluster.GrainFactory.GetGrain<IMonsterGrain>(id);
//            string text = await monster.Kill(this.room, 0);
//            Assert.Contains("100 health left!", text);
//            
//            Thread.Sleep(5000);
//            text = await monster.Kill(this.room, 0);
//            Assert.Contains("110 health left!", text);
            
//            //Necessary to dispose timers
//            string bossText = await this.boss.Kill(this.room, 999);
//            Assert.Contains("Patches the one-eyed demon has been slain!", bossText);
//        }
    }
}