using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    public class MonsterRoomInteractionTests : TestKitBase
    {
        private readonly TestCluster _cluster;
        private Mock<MonsterGrain> monster;
        private Mock<RoomGrain> room;
        

        public MonsterRoomInteractionTests(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
            monster = new Mock<MonsterGrain>();
            room = new Mock<RoomGrain>();
        }

        [Fact]
        public async void MonsterSetRoomGrainTest()
        {
            MonsterInfo mi = new MonsterInfo(){Name = "testMonster"};
            
            await this.monster.Object.SetInfo(mi);
            await this.monster.Object.SetRoomGrain(this.room.Object);
            var mon = await this.room.Object.FindMonster("testMonster");
            
            Assert.Equal(mi, mon);
        }
        
        [Fact]
        public async void MonsterSetRoomGrainAlreadyInRoomTest()
        {
            MonsterInfo mi = new MonsterInfo(){Name = "testMonster", Id = 0, KilledBy = {}};
            IMonsterGrain monster = _cluster.GrainFactory.GetGrain<IMonsterGrain>(mi.Id);
            await monster.SetInfo(mi);
            RoomInfo ri = new RoomInfo() {Directions = new Dictionary<string, long>(){{"north", 2}, {"south", 3}, {"east", 4}, {"west", 5}}};
            IRoomGrain room = _cluster.GrainFactory.GetGrain<IRoomGrain>(0);
            await room.SetInfo(ri);

            await monster.SetRoomGrain(room);
            MonsterInfo mon = await room.FindMonster("testMonster");
            Assert.Equal(mi.Name, mon.Name);
            Assert.Equal(mi.Id, mon.Id);
            Assert.Equal(mi.KilledBy, mon.KilledBy);
            
            Thread.Sleep(21000);
            mon = await this.room.Object.FindMonster("testMonster");
            Assert.Null(mon);
            
            var exitRoom = _cluster.GrainFactory.GetGrain<IRoomGrain>(5);
            mon = await exitRoom.FindMonster("testMonster");
            Assert.Equal(mi.Name, mon.Name);
            Assert.Equal(mi.Id, mon.Id);
            Assert.Equal(mi.KilledBy, mon.KilledBy);
        }
    }
}