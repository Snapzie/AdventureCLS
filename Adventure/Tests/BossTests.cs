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
        public async void SpawnAddsBuffTest()
        {
            //Arrange
            PlayerInfo playerInfo = new PlayerInfo();
            Mock<IMonsterGrain> add = new Mock<IMonsterGrain>();
            this.boss.Setup(b => b.GrainFactory.GetGrain<IMonsterGrain>(It.IsAny<long>(), "AdventureGrains.Monster")).Returns(add.Object);
            await this.boss.Object.SetRoomGrain(this.room.Object);
            this.room.Setup(r => r.GetTargetsForMonster()).Returns(Task.FromResult(new List<PlayerInfo> { playerInfo }));

            //Act
            string res = await this.boss.Object.Kill(this.room.Object, 10);
            //Assert 
            Assert.Equal(" took 10 damage. He now has 190 health left!", res);

            //Act
            await this.boss.Object.SpawnAdds(this.room.Object);
            string res2 = await this.boss.Object.Kill(this.room.Object, 10);
            //Assert
            Assert.Equal(" took 5 damage. He now has 185 health left!", res2);
        }

        [Fact]
        public async void UpdateAddsBuffTest()
        {
            //Arrange
            PlayerInfo playerInfo = new PlayerInfo();
            MonsterInfo monsterInfo = new MonsterInfo();
            monsterInfo.Id = 100;
            Mock<IMonsterGrain> add = new Mock<IMonsterGrain>();
            this.boss.Setup(b => b.GrainFactory.GetGrain<IMonsterGrain>(It.IsAny<long>(), "AdventureGrains.Monster")).Returns(add.Object);
            this.room.Setup(r => r.GetTargetsForMonster()).Returns(Task.FromResult(new List<PlayerInfo> { playerInfo }));
            await this.boss.Object.SetRoomGrain(this.room.Object);
            await this.boss.Object.SpawnAdds(this.room.Object);

            //Act
            string res = await this.boss.Object.Kill(this.room.Object, 10);
            //Assert
            Assert.Equal(" took 5 damage. He now has 195 health left!", res);

            //Act
            await this.boss.Object.UpdateAdds(monsterInfo);
            string res2 = await this.boss.Object.Kill(this.room.Object, 10);
            //Assert
            Assert.Equal(" took 10 damage. He now has 185 health left!", res2);
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

        [Fact]
        public async void KillTestAddsActive()
        {
            //Arrange
            PlayerInfo playerInfo = new PlayerInfo();
            Mock<IMonsterGrain> add = new Mock<IMonsterGrain>();
            this.boss.Setup(b => b.GrainFactory.GetGrain<IMonsterGrain>(It.IsAny<long>(), "AdventureGrains.Monster")).Returns(add.Object);
            await this.boss.Object.SetRoomGrain(this.room.Object);
            this.room.Setup(r => r.GetTargetsForMonster()).Returns(Task.FromResult(new List<PlayerInfo> { playerInfo }));
            await this.boss.Object.SpawnAdds(this.room.Object);

            //Act
            string res = await this.boss.Object.Kill(this.room.Object, 1);
            //Assert
            Assert.Equal(" took 0 damage. He now has 200 health left!", res);

            //Act
            string res2 = await this.boss.Object.Kill(this.room.Object, 0);
            //Assert
            Assert.Equal(" took 0 damage. He now has 200 health left!", res2);

            //Act
            string res3 = await this.boss.Object.Kill(this.room.Object, -5);
            //Assert
            Assert.Equal(" took -2 damage. He now has 202 health left!", res3);
        }
    }
}