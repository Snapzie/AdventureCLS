using AdventureGrainInterfaces;
using Orleans;
using Orleans.Runtime;
using Orleans.TestingHost;
using Orleans.TestKit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;

namespace Tests
{
    [Collection(ClusterCollection.Name)]
    public class PlayerIntegrationTests : TestKitBase
    {
        private readonly TestCluster _cluster;
        private IPlayerGrain player;
        private IMonsterGrain monster;
        private IRoomGrain room;
        //private Guid playerGuid;
        private MonsterInfo monsterInfo;
        private Random rand = new Random();
        private long monsterId;

        public PlayerIntegrationTests(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
            //_cluster.Deploy();
            //playerGuid = new Guid();
            monsterId = rand.Next();
            player = _cluster.GrainFactory.GetGrain<IPlayerGrain>(Guid.NewGuid());
            room = _cluster.GrainFactory.GetGrain<IRoomGrain>(monsterId);
            //monster = _cluster.GrainFactory.GetGrain<IMonsterGrain>(0);

            //player.SetRoomGrain(room).Wait();

            monsterInfo = new MonsterInfo();
            monsterInfo.Id = monsterId;
            monsterInfo.KilledBy = new List<long> { 0 };
            monsterInfo.Name = "testMonster";
            //monster.SetInfo(monsterInfo).Wait();
            //monster.SetRoomGrain(room).Wait();
        }

        public async void Dispose()
        {
            await this.monster?.Kill(this.room, 999);
            await this.player?.Die();
            //await this.player.TakeDamage(this.room, 999);
            this._cluster.StopAllSilos();
        }

        [Fact]
        public async void PlayerFireballTestMonster()
        {
            //Arrange
            await player.SetRoomGrain(room);
            monster = _cluster.GrainFactory.GetGrain<IMonsterGrain>(monsterId);
            await monster.SetInfo(monsterInfo);
            await monster.SetRoomGrain(room);

            //Act
            string res = await player.Play("fireball testMonster");

            //Assert
            Assert.Equal("testMonster took 50 damage. He now has 50 health left!", res);
                
        }

        [Fact]
        public async void PlayerFireballTestPlayer()
        {
            //Arrange
            await player.SetRoomGrain(room);

            //Act
            string res = await player.Play("fireball nobody");

            //Assert
            Assert.Equal("nobody took 50 damage and now has 45 health left!", res);

        }

        [Fact]
        public async void PlayerGoRoomTest()
        {
            //Arrange
            await player.SetRoomGrain(room);
            IRoomGrain newRoom = _cluster.GrainFactory.GetGrain<IRoomGrain>(123);
            RoomInfo newRoomInfo = new RoomInfo();
            newRoomInfo.Description = "some desc";
            newRoomInfo.Id = 123;
            newRoomInfo.Directions = new Dictionary<string, long> { };
            await newRoom.SetInfo(newRoomInfo);

            RoomInfo roomInfo = new RoomInfo();
            roomInfo.Id = monsterId;
            roomInfo.Directions = new Dictionary<string, long> { { "north", 123 } };
            await this.room.SetInfo(roomInfo);

            //Act
            string res = await this.player.Play("north");

            //Assert
            Assert.Contains("some desc", res);
        }

        [Fact]
        public async void PlayerGoNoAdjacentRoomTest()
        {
            //Arrange
            await player.SetRoomGrain(room);

            //Act
            string res = await this.player.Play("north");

            //Assert
            Assert.Equal("You cannot go in that direction.", res);
        }

        [Fact]
        public async void PlayerTakeTest()
        {
            //Arrange
            await player.SetRoomGrain(room);
            Thing knife = new Thing();
            knife.Name = "knife";
            await room.Drop(knife);

            //Act
            string res = await this.player.Play("take knife");

            //Assert
            Assert.Equal("Okay.", res);
            Assert.DoesNotContain("knife", await this.room.Description(new PlayerInfo()));
        }

        [Fact]
        public async void PlayerDropTest()
        {
            //Arrange
            await player.SetRoomGrain(room);
            Thing knife = new Thing();
            knife.Name = "knife";
            await this.room.Drop(knife);
            await this.player.Play("take knife");

            //Act
            string res = await this.player.Play("drop knife");

            //Assert
            Assert.Equal("Okay.", res);
            Assert.Contains("knife", await this.room.Description(new PlayerInfo()));
        }

        [Fact]
        public async void PlayerKillPlayerTest()
        {
            //Arrange
            await player.SetRoomGrain(room);
            Thing knife = new Thing();
            knife.Name = "knife";
            knife.Category = "weapon";
            await this.room.Drop(knife);
            await this.player.Play("take knife");
            PlayerInfo pi = new PlayerInfo();
            pi.Key = new Guid();
            pi.Name = "testPlayer";
            IPlayerGrain enemyPlayer = _cluster.GrainFactory.GetGrain<IPlayerGrain>(new Guid());
            await enemyPlayer.SetName("testPlayer");
            await enemyPlayer.SetRoomGrain(this.room);

            //Act
            string res = await player.Play("kill testPlayer");

            //Assert
            Assert.Equal("testPlayer is now dead.", res);
        }

        [Fact]
        public async void PlayerKillMonsterTest()
        {
            //Arrange
            await player.SetRoomGrain(room);
            Thing knife = new Thing();
            knife.Name = "knife";
            knife.Category = "weapon";
            knife.Id = 0;
            await this.room.Drop(knife);
            await this.player.Play("take knife");
            MonsterInfo mi = new MonsterInfo();
            mi.Id = 0;
            mi.Name = "testMonster";
            mi.KilledBy = new List<long> { 0 };
            IMonsterGrain enemyMonster = _cluster.GrainFactory.GetGrain<IMonsterGrain>(0);
            await enemyMonster.SetInfo(mi);
            await enemyMonster.SetRoomGrain(this.room);

            //Act
            string res = await this.player.Play("kill testMonster");
            //Assert
            Assert.Equal("testMonster took 20 damage. He now has 80 health left!", res);

            //Act
            await this.player.Play("kill testMonster");
            await this.player.Play("kill testMonster");
            await this.player.Play("kill testMonster");
            string res2 = await this.player.Play("kill testMonster");

            //Assert
            Assert.Equal("testMonster is dead.", res2);
        }

        [Fact]
        public async void PlayerLookTest()
        {
            //Arrange
            await player.SetRoomGrain(room);

            //Act
            string res = await this.player.Play("look");

            //Assert
            Assert.Contains("Your health is: 95", res);
        }

        [Fact]
        public async void MonsterAttackPlayerTest()
        {
            //Arrange
            await player.SetRoomGrain(room);
            monster = _cluster.GrainFactory.GetGrain<IMonsterGrain>(monsterId);
            await monster.SetInfo(monsterInfo);
            await monster.SetRoomGrain(room);

            //Assert
            Assert.Equal(95, await this.player.GetHealth());

            //Act
            Thread.Sleep(2020);
            //Assert
            Assert.Equal(85, await this.player.GetHealth());

            //Act
            Thread.Sleep(10020);
            //Assert
            Assert.Equal(75, await this.player.GetHealth());
        }


        [Fact]
        public async void PlayerTakeDamageTest()
        {
            //Arrange
            await player.SetRoomGrain(room);

            //Act
            await this.player.TakeDamage(this.room, -5);
            //Assert
            Assert.Equal(100, await this.player.GetHealth());

            //Act
            await this.player.TakeDamage(this.room, 1);
            //Assert
            Assert.Equal(99, await this.player.GetHealth());

            //Act
            await this.player.TakeDamage(this.room, 0);
            //Assert
            Assert.Equal(99, await this.player.GetHealth());

            //Act
            await this.player.TakeDamage(this.room, int.MinValue);
            //Assert
            Assert.Equal(-2147483549, await this.player.GetHealth());
        }

        [Fact]
        public async void PlayerDescriptionNoPlayersNoItemsNoMonstersTest()
        {
            //Act
            string desc = await room.Description(new PlayerInfo());

            //Assert
            Assert.Equal("Your health is: 100\n", desc);
        }

        [Fact]
        public async void PlayerDescriptionWithPlayersNoItemsNoMonstersTest()
        {
            //Arrange
            await this.player.SetRoomGrain(this.room);

            string desc = await room.Description(new PlayerInfo());

            Assert.Equal("Beware! These guys are in the room with you:\n  nobody\nYour health is: 100\n", desc);
        }

        [Fact]
        public async void PlayerDescriptionNoPlayersNoItemsWithMonstersTest()
        {
            //Arrange
            monster = _cluster.GrainFactory.GetGrain<IMonsterGrain>(monsterId);
            await this.monster.SetInfo(monsterInfo);
            await this.monster.SetRoomGrain(this.room);

            //Act
            string desc = await room.Description(new PlayerInfo());

            //Assert
            Assert.Equal("Beware! These guys are in the room with you:\n  testMonster\nYour health is: 100\n", desc);
        }

        [Fact]
        public async void PlayerDescriptionNoPlayersWithItemsNoMonstersTest()
        {
            //Arrange
            Thing t = new Thing() { Name = "testThing" };
            await this.room.Drop(t);

            //Act
            string desc = await room.Description(new PlayerInfo());

            //Assert
            Assert.Equal("The following things are present:\n  testThing\nYour health is: 100\n", desc);
        }

    }
}
