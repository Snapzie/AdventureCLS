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
    public class MonsterIntegrationTests : TestKitBase
    {
        private readonly TestCluster _cluster;
        private IMonsterGrain monster;
        private IRoomGrain room;
        

        public MonsterIntegrationTests(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
            monster = _cluster.GrainFactory.GetGrain<IMonsterGrain>(0);
            room = _cluster.GrainFactory.GetGrain<IRoomGrain>(0);
        }

        [Fact]
        public async void MonsterSetRoomGrainTest()
        {
            //Arrange
            MonsterInfo mi = new MonsterInfo(){Name = "testMonster"};
            await this.monster.SetInfo(mi);
            await this.monster.SetRoomGrain(this.room);
            //Act
            var mon = await this.room.FindMonster("testMonster");
            //Assert
            Assert.Equal(mi.Name, mon.Name);
            Assert.Equal(mi.Id, mon.Id);
            Assert.Equal(mi.KilledBy, mon.KilledBy);
        }
        
        [Fact]
        public async void MonsterMoveTest()
        {
            //Arrange
            MonsterInfo mi = new MonsterInfo(){Name = "testMonster", Id = 0, KilledBy = {}};
            IMonsterGrain monster = _cluster.GrainFactory.GetGrain<IMonsterGrain>(mi.Id);
            await monster.SetInfo(mi);
            RoomInfo ri = new RoomInfo() {Directions = new Dictionary<string, long>(){{"north", 2}, {"south", 3}, {"east", 4}, {"west", 5}}};
            await this.room.SetInfo(ri);
            await monster.SetRoomGrain(this.room);
            MonsterInfo mon = await this.room.FindMonster("testMonster");
            Assert.Equal(mi.Name, mon.Name);
            Assert.Equal(mi.Id, mon.Id);
            Assert.Equal(mi.KilledBy, mon.KilledBy);
            //Act
            Thread.Sleep(21000);
            mon = await this.room.FindMonster("testMonster");
            Assert.Null(mon);
            var exitRoom = _cluster.GrainFactory.GetGrain<IRoomGrain>(5);
            mon = await exitRoom.FindMonster("testMonster");
            //Assert
            Assert.Equal(mi.Name, mon.Name);
            Assert.Equal(mi.Id, mon.Id);
            Assert.Equal(mi.KilledBy, mon.KilledBy);
        }
    }
}