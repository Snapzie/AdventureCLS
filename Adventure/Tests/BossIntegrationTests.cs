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
    public class BossIntegrationTests : TestKitBase
    {
        private readonly TestCluster _cluster;
        private IBossGrain boss;
        private IRoomGrain room;
        private IPlayerGrain player;


        public BossIntegrationTests()
        {
            var testClusterBuilder = new TestClusterBuilder();
            testClusterBuilder.AddSiloBuilderConfigurator<TestSiloBuilderConfigurator>();
            TestCluster Cluster = testClusterBuilder.Build();
            Cluster.Deploy();
            
            _cluster = Cluster;
            boss = _cluster.GrainFactory.GetGrain<IBossGrain>(0);
            room = _cluster.GrainFactory.GetGrain<IRoomGrain>(0);
            player = _cluster.GrainFactory.GetGrain<IPlayerGrain>(Guid.NewGuid());
        }

        [Fact]
        public async void BossSetRoomGrainTest()
        {
            //Arrange
            await this.boss.SetInfo();
            //Act
            await this.boss.SetRoomGrain(this.room);
            var bo = await this.room.GetBoss();
            //Assert
            Assert.Equal("Patches the one-eyed demon", bo.Name);
            Assert.Equal(0, bo.Id);
            Assert.Single(bo.KilledBy);
        }
        
        [Fact]
        public async void RoomBossExitTest()
        {
            //Arrange
            await this.boss.SetInfo();
            await this.boss.SetRoomGrain(this.room);
            var bo = await this.room.GetBoss();
            Assert.Equal(0, bo.Id);
            //Act
            await this.boss.Kill(this.room, 200);
            bo = await this.room.GetBoss();
            //Arrange
            Assert.Null(bo);
        }

        [Fact]
        public async void BossSpawnAddsTest()
        {
            //Arrange
            await this.player.SetRoomGrain(this.room);
            await this.boss.SetRoomGrain(this.room);
            Assert.Null(await this.room.FindMonster("one-and-a-half-eyed demon"));
            //Act
            Thread.Sleep(6000);
            MonsterInfo foundMonster = await this.room.FindMonster("one-and-a-half-eyed demon");
            //Act
            Assert.Equal(100, foundMonster.Id);
            Assert.Equal("one-and-a-half-eyed demon", foundMonster.Name);
        }

        [Fact]
        public async void BossKillKnifeTest()
        {
            //Arrange
            await this.boss.SetInfo();
            await this.room.Drop(new Thing() {Category = "weapon", Name = "knife", Id = 1});
            await this.player.SetRoomGrain(this.room);
            await this.boss.SetRoomGrain(this.room);
            //Act
            await this.player.Play("take knife");
            string res = await this.player.Play("kill Patches");
            //Assert
            Assert.Equal("Patches the one-eyed demon took 20 damage. He now has 180 health left!", res);
        }
        
        [Fact]
        public async void BossKillWithAddTest() //Duplicate with 20 dmg for ensuring correct damage values for other synthesis?
        {
            //Arrange
            await this.boss.SetInfo();
            await this.room.Drop(new Thing() {Category = "weapon", Name = "knife", Id = 1});
            await this.player.SetRoomGrain(this.room);
            await this.boss.SetRoomGrain(this.room);
            await this.player.Play("take knife");
            Thread.Sleep(6000);
            Assert.NotNull(await this.room.FindMonster("one-and-a-half-eyed demon"));
            //Act
            string res = await this.player.Play("kill Patches");
            //Assert
            Assert.Equal("Patches the one-eyed demon took 10 damage. He now has 190 health left!", res);
        }
        
        [Fact]
        public async void BossUpdateAddsTest()
        {
            //Arrange
            await this.boss.SetInfo();
            await this.room.Drop(new Thing() {Category = "weapon", Name = "knife", Id = 1});
            await this.player.SetRoomGrain(this.room);
            await this.boss.SetRoomGrain(this.room);
            await this.player.Play("take knife");
            Thread.Sleep(6000);
            Assert.NotNull(await this.room.FindMonster("one-and-a-half-eyed demon"));
            string res = await this.player.Play("kill Patches");
            Assert.Equal("Patches the one-eyed demon took 10 damage. He now has 190 health left!", res);
            IMonsterGrain monster = _cluster.GrainFactory.GetGrain<IMonsterGrain>(100);
            await monster.Kill(this.room, 100);
            //Act
            res = await this.player.Play("kill Patches");
            //Assert
            Assert.Equal("Patches the one-eyed demon took 20 damage. He now has 170 health left!", res);
        }

        [Fact]
        public async void BossHealAddsTest()
        {
            //Arrange
            await this.player.SetRoomGrain(this.room);
            await this.boss.SetRoomGrain(this.room);
            Thread.Sleep(11000);
            MonsterInfo mi = await this.room.FindMonster("one-and-a-half-eyed demon");
            Assert.Equal(100, mi.Id);
            IMonsterGrain monster = _cluster.GrainFactory.GetGrain<IMonsterGrain>(100);
            //Act
            string res = await monster.Kill(this.room, 0);
            //Assert
            Assert.Equal("one-and-a-half-eyed demon took 0 damage. He now has 110 health left!", res);
        }
        
        [Fact]
        public async void BossAttackNoRoarTest()
        {
            //Arrange
            await this.player.SetRoomGrain(this.room); //Blizzard dealing 5 damage
            await this.boss.SetInfo();
            await this.boss.SetRoomGrain(this.room);
            Thread.Sleep(6000);
            IMonsterGrain monster = _cluster.GrainFactory.GetGrain<IMonsterGrain>(100);
            await monster.Kill(this.room, 100);
            Thread.Sleep(5000);
            //Act
            string res = await this.player.Play("look");
            //Assert
            Assert.Equal("Beware! These guys are in the room with you:\n  Patches the one-eyed demon\nYour health is: 93\n", res);
        }
        
        [Fact]
        public async void BossAttackRoarTest()
        {
            //Arrange
            await this.player.SetRoomGrain(this.room); //Blizzard dealing 5 damage
            await this.boss.SetInfo();
            await this.boss.SetRoomGrain(this.room);
            Thread.Sleep(6000);
            IMonsterGrain monster = _cluster.GrainFactory.GetGrain<IMonsterGrain>(100);
            await monster.Kill(this.room, 100);
            await this.player.Play("roar");
            Thread.Sleep(5000);
            //Act
            string res = await this.player.Play("look");
            //Assert
            Assert.Equal("Beware! These guys are in the room with you:\n  Patches the one-eyed demon\nYour health is: 94\n", res);
        }
        
        [Fact]
        public async void PlayerFireballBossTest()
        {
            //Arrange
            await this.player.SetRoomGrain(this.room);
            await this.boss.SetInfo();
            await this.boss.SetRoomGrain(this.room);
            //Act
            string res = await this.player.Play("fireball patches");
            //Assert
            Assert.Equal("Patches the one-eyed demon took 50 damage. He now has 150 health left!", res);
        }
        
        [Fact]
        public async void DescriptionNoPlayersNoItemsWithBossNoMonstersTest()
        {
            //Arrange
            await this.boss.SetInfo();
            await this.boss.SetRoomGrain(this.room);
            //Act
            string desc = await room.Description(new PlayerInfo());
            //Asserts
            Assert.Equal("Beware! These guys are in the room with you:\n  Patches the one-eyed demon\nYour health is: 100\n", desc);
        }
    }
}