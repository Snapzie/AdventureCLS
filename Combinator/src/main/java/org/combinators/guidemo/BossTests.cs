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
        private Mock<IRoomGrain> room;
        private PlayerInfo playerInfo = new PlayerInfo();
        private Mock<BossGrain> boss;

        public BossTests(ClusterFixture fixture)
        { 
            _cluster = fixture.Cluster;

            //Room Setup
            int num = new Random().Next();
            this.room = new Mock<IRoomGrain>();
            RoomInfo ri = new RoomInfo();
            ri.Description = "This is a test room";
            ri.Directions = new Dictionary<string, long>();
            ri.Id = num;
            ri.Name = "TestRoom";

            //Player Setup
            this.playerInfo.Key = new Guid();
            this.playerInfo.Name = "TestPlayer";

            //Boss Setup
            this.boss = new Mock<BossGrain>();
        }

        [Fact]
        public async void KillTest()
        {
            //Arrange
            await this.boss.Object.SetRoomGrain(this.room.Object);

            //Act
            string res = await this.boss.Object.Kill(this.room.Object, 1);
            //Assert
            Assert.Equal(" took 1 damage. He now has 199 health left!", res);

            //Act
            string res2 = await this.boss.Object.Kill(this.room.Object, -1);
            //Assert
            Assert.Equal(" took -1 damage. He now has 200 health left!", res2);

            //Act
            string res3 = await this.boss.Object.Kill(this.room.Object, 0);
            //Assert
            Assert.Equal(" took 0 damage. He now has 200 health left!", res3);

            //Act
            string res4 = await this.boss.Object.Kill(this.room.Object, 201);
            //Assert
            Assert.Equal(" has been slain!", res4);
        }
    }
}