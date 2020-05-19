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
    public class BossIntegrationTests : TestKitBase
    {
        private readonly TestCluster _cluster;
        private IMonsterGrain monster;
        private IRoomGrain room;


        public BossIntegrationTests(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
            monster = _cluster.GrainFactory.GetGrain<IMonsterGrain>(0);
            room = _cluster.GrainFactory.GetGrain<IRoomGrain>(0);
        }

        [Fact]
        public async void MonsterSetRoomGrainTest()
        {
            MonsterInfo mi = new MonsterInfo() {Name = "testMonster"};

            await this.monster.SetInfo(mi);
            await this.monster.SetRoomGrain(this.room);
            var mon = await this.room.FindMonster("testMonster");

            Assert.Equal(mi.Name, mon.Name);
            Assert.Equal(mi.Id, mon.Id);
            Assert.Equal(mi.KilledBy, mon.KilledBy);
        }
    }
}