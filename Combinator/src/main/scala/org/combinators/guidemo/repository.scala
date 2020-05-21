package org.combinators.guidemo

import java.net.URL

import org.combinators.cls.interpreter.{combinator, ReflectedRepository}
import org.combinators.cls.types.{Kinding, Type, Variable}
import org.combinators.cls.types.syntax._
import org.combinators.cls.git.{EmptyInhabitationBatchJobResults, Results, ResultLocation}
import org.combinators.templating.twirl.Java

import org.combinators.guidemo.Helpers._

import org.combinators.guidemo.domain.{AdventureGame, AbilityTypes}
import org.combinators.templating.persistable.Persistable

import org.combinators.guidemo.domain.WeatherTypes;
import org.combinators.guidemo.domain.BossAbilityTypes;

class Repository(adventureGame: AdventureGame) {
  lazy val ability = Variable("ability")
  lazy val abilityKinding: Kinding =
  Kinding(ability)
    .addOption('fireball).addOption('roar).addOption('none)

  lazy val boss = Variable("boss")
  lazy val bossKinding: Kinding =
  Kinding(boss)
    .addOption('variationWithBoss).addOption('variationWithoutBoss)

//################################## PLAYER ######################################################
  @combinator object PlayerGrain {
      def apply(caseString: String,
            ability: String): MyResult = {
          val file = MyResult(readFile("PlayerGrain.cs"), "PlayerGrain.cs")
          addArbCode(file, caseString, "switch (verb)", '{')
          addArbCode(file, ability, "PlayerGrain", '{')
          file
      }
      val semanticType: Type =
        'case(ability) =>: 'ability(ability) =>: 'player(ability)
  }

  @combinator object PlayerTests {
      def apply(testAbility: String): MyResult = {
          val file = MyResult(readFile("PlayerTests.cs"), "PlayerTests.cs")
          addArbCode(file, testAbility, "public class PlayerTests", '{')
          file
      }
      val semanticType: Type =
        'testAbility(ability, boss) =>: 'playerTest(ability, boss)
  }

  @combinator object abilityFireball {
    def apply(): String = {
        """
        private bool fireballCD = false;

        private async Task<string> Fireball(string target)
        {
            this.fireballCD = true;
            IDisposable fcd = RegisterTimer((_) => FireballCooldown(), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(-1));
            
            var player = await this.roomGrain.FindPlayer(target);
            if (player != null)
            {
                await GrainFactory.GetGrain<IPlayerGrain>(player.Key, "AdventureGrains.Player").TakeDamage(this.roomGrain, 50);
                return $"{player.Name} took 50 damage and now has {await GrainFactory.GetGrain<IPlayerGrain>(player.Key, "AdventureGrains.Player").GetHealth()} health left!";
            }

            var monster = await this.roomGrain.FindMonster(target);
            if (monster != null)
            {
                string res = await GrainFactory.GetGrain<IMonsterGrain>(monster.Id, "AdventureGrains.Monster").Kill(this.roomGrain, 50);
                return res;
            }

            var boss = await this.roomGrain.GetBoss();
            if (boss != null)
            {
                string res = await GrainFactory.GetGrain<IBossGrain>(boss.Id, "AdventureGrains.Boss").Kill(this.roomGrain, 50);
                return res;
            }
            this.fireballCD = false;
            fcd?.Dispose();
            return "I can't see " + target + " here. Are you sure?";
        }

        private Task FireballCooldown()
        {
            this.fireballCD = false;
            return Task.CompletedTask;
        }"""
    }
      val semanticType: Type = 'ability('fireball)
  }

  @combinator object abilityRoar {
    def apply(): String = {
        """
        private bool roarCD = false;

        private Task<string> Roar()
        {
            this.roarCD = true;
            this.roarActive = true;
            RegisterTimer((_) => RoarActive(), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(-1));
            RegisterTimer((_) => RoarCooldown(), null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(-1));
            return Task.FromResult("Roar has been activated!");
        }
        
        private Task RoarActive()
        {
            this.roarActive = false;
            return Task.CompletedTask;
        }

        private Task RoarCooldown()
        {
            this.roarCD = false;
            return Task.CompletedTask;
        }"""
    }
      val semanticType: Type = 'ability('roar)
  }

  @combinator object abilityNone {
    def apply(): String = {""}
    val semanticType: Type = 'ability('none)
  }

  @combinator object caseFireball {
      def apply(): String = {
          """
                case "fireball":
                    if (words.Length == 1)
                        return "Fireball what?";
                    if (fireballCD)
                    {
                        return "Fireball is on cooldown";
                    }
                    target = command.Substring(verb.Length + 1);
                    if (target == "")
                    {
                        return "Fireball what?";
                    }
                    return await Fireball(target);"""
      }
        val semanticType: Type = 'case('fireball)
  }

  @combinator object caseRoar {
      def apply(): String = {
          """
                case "roar":
                    if (words.Length > 1)
                        return "Can not roar others";
                    if (roarCD)
                    {
                        return "Roar is on cooldown";
                    }
                    return await Roar();"""
      }
        val semanticType: Type = 'case('roar)
  }

  @combinator object caseNone {
    def apply(): String = {""}
    val semanticType: Type = 'case('none)
  }

  @combinator object testNoBoss {
      def apply(): String = {""}
        val semanticType: Type = 'bossPresent(ability, 'variationWithoutBoss)
  }

  @combinator object testBoss {
      def apply(): String = {
        """
        [Fact]
        public async void KillBossTest()
        {
            //Arrange
            MonsterInfo bi = new MonsterInfo();
            bi.Id = 0;
            bi.KilledBy = new List<long> { 0 };
            Thing knife = new Thing();
            knife.Name = "knife";
            knife.Category = "weapon";
            knife.Id = 0;
            room.Setup(r => r.FindThing(It.IsAny<string>())).Returns(Task.FromResult(knife));
            room.Setup(r => r.Take(It.IsAny<Thing>())).Returns(Task.FromResult(knife));
            await this.player.Object.Play("take knife");
            Mock<IBossGrain> enemyBoss = new Mock<IBossGrain>();
            enemyBoss.Setup(eb => eb.Kill(It.IsAny<IRoomGrain>(), It.IsAny<int>())).Returns(Task.FromResult("Ouch!"));
            room.Setup(r => r.GetBoss()).Returns(Task.FromResult(bi));
            player.Setup(p => p.GrainFactory.GetGrain<IBossGrain>(It.IsAny<long>(), "AdventureGrains.Boss")).Returns(enemyBoss.Object);

            //Act
            string res = await player.Object.Play("kill testBoss");

            //Assert
            Assert.Equal("Ouch!", res);
        }"""
      }
        val semanticType: Type = 'boss('variationWithBoss)
  }

  @combinator object testRoarBoss {
      def apply(boss: String): String = {boss + ""}
        val semanticType: Type = 'boss('variationWithBoss) =>: 'bossPresent('roar, 'variationWithBoss)
  }

  @combinator object tesNoneBoss {
      def apply(boss: String): String = {boss + ""}
        val semanticType: Type = 'boss('variationWithBoss) =>: 'bossPresent('none, 'variationWithBoss)
  }

  @combinator object testFireballBoss {
      def apply(boss: String): String = {
        boss +
        """
        [Fact]
        public async void FireballTestBoss()
        {
            //Arrange
            MonsterInfo bossInfo = new MonsterInfo();
            bossInfo.Id = 666;
            bossInfo.Name = "testBoss";
            var boss = new Mock<IBossGrain>();
            boss.Setup(b => b.SetInfo()).Returns(Task.FromResult(bossInfo));
            boss.Setup(b => b.Kill(It.IsAny<IRoomGrain>(), It.IsAny<int>())).Returns(Task.FromResult("Ouch!"));

            player.Setup(p => p.GrainFactory.GetGrain<IBossGrain>(It.IsAny<long>(), "AdventureGrains.Boss")).Returns(boss.Object);
            room.Setup(r => r.GetBoss()).Returns(Task.FromResult(bossInfo));

            //Act
            string res = await player.Object.Play("fireball testBoss");

            //Assert
            Assert.Equal("Ouch!", res);
        }"""
      }
        val semanticType: Type = 'boss('variationWithBoss) =>: 'bossPresent('fireball, 'variationWithBoss)
  }

  @combinator object testFireball {
      def apply(bossPresent: String): String = {
        """
        [Fact]
        public async void FireballTestMonster()
        {
            //Arrange
            monster.Setup(x => x.Kill(It.IsAny<IRoomGrain>(), It.IsAny<int>()))
                .Returns(Task.FromResult("testMonster took 50 damage. He now has 50 health left!"));

            player.Setup(p => p.GrainFactory.GetGrain<IMonsterGrain>(It.IsAny<long>(), "AdventureGrains.Monster")).Returns(monster.Object);
            room.Setup(x => x.FindMonster(It.IsAny<string>())).Returns(Task.FromResult(mi));

            //Act
            string res = await player.Object.Play("fireball testMonster");
            //Assert
            Assert.Equal("testMonster took 50 damage. He now has 50 health left!", res);
        }
        
        [Fact]
        public async void FireballTestPlayer()
        {
            //Arrange
            var enemyPlayer = new Mock<IPlayerGrain>();
            PlayerInfo pi = new PlayerInfo();
            pi.Key = new Guid();
            pi.Name = "testPlayer";

            player.Setup(x => x.GrainFactory.GetGrain<IPlayerGrain>(It.IsAny<Guid>(),
                "AdventureGrains.Player")).Returns(enemyPlayer.Object);
            
            room.Setup(x => x.FindPlayer(It.IsAny<string>())).Returns(Task.FromResult<PlayerInfo>(pi));

            enemyPlayer.Setup(x => x.TakeDamage(It.IsAny<IRoomGrain>(), It.IsAny<int>()))
                .Returns(Task.FromResult("testPlayer took 50 damage and now has 0 health left!"));

            //Act
            string res = await player.Object.Play("fireball testPlayer");
            //Assert
            Assert.Equal("testPlayer took 50 damage and now has 0 health left!", res);
        }

        [Fact]
        public async void FireballTestNoOne()
        {
            //Act
            string res = await player.Object.Play("fireball");
            //Assert
            Assert.Equal("Fireball what?", res);

            //Act
            string res2 = await player.Object.Play("fireball No One");
            //Assert
            Assert.Equal("I can't see No One here. Are you sure?", res2);
        }

        [Fact]
        public async void FireballTestCooldown()
        {
            //Arrange
            Func<object, Task> action = null;
            object state = null;
            TimeSpan dueTime = TimeSpan.FromSeconds(100);
            TimeSpan period = TimeSpan.FromSeconds(100);
            player.Setup(x => x.RegisterTimer(It.IsAny<Func<object, Task>>(),
                    It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
                .Callback<Func<object, Task>, object, TimeSpan, TimeSpan>((a, b, c, d) =>
                {
                    action = a;
                    state = b;
                    dueTime = c;
                    period = d;
                }).Returns(Mock.Of<IDisposable>());
            player.Setup(p => p.GrainFactory.GetGrain<IMonsterGrain>(It.IsAny<long>(), "AdventureGrains.Monster")).Returns(monster.Object);
            room.Setup(r => r.FindMonster(It.IsAny<string>())).Returns(Task.FromResult(mi));
            await player.Object.Play("Fireball testMonster");

            //Act
            string res = await player.Object.Play("Fireball testMonster");

            //Assert
            Assert.Equal("Fireball is on cooldown", res);
            Assert.NotNull(action);
            Assert.Equal(10, dueTime.TotalSeconds);
            Assert.Equal(-1, period.TotalSeconds);
            Assert.Null(state);
        }""" + bossPresent
      }
        val semanticType: Type = 'bossPresent('fireball, boss) =>: 'testAbility('fireball, boss)
  }

  @combinator object testRoar {
      def apply(bossPresent: String): String = {
        """
        [Fact]
        public async void RoarTest()
        {
            //Act
            string res2 = await this.player.Object.Play("roar");

            //Assert
            Assert.Equal("Roar has been activated!", res2);

            //Act
            await this.player.Object.TakeDamage(this.room.Object, 50);
            //Assert
            Assert.Equal(75, await this.player.Object.GetHealth());

            //Act
            await this.player.Object.TakeDamage(this.room.Object, 0);
            //Assert
            Assert.Equal(75, await this.player.Object.GetHealth());

            //Act
            await this.player.Object.TakeDamage(this.room.Object, -4);
            //Assert
            Assert.Equal(77, await this.player.Object.GetHealth());

            //Act
            await this.player.Object.TakeDamage(this.room.Object, 1);
            //Assert
            Assert.Equal(77, await this.player.Object.GetHealth());
        }

        [Fact]
        public async void RoarSomeoneTest()
        {
            //Act
            string res = await this.player.Object.Play("roar someone");
            //Assert
            Assert.Equal("Can not roar others", res);
        }

        [Fact]
        public async void RoarCooldownTest()
        {
            //Arrange
            Func<object, Task> action = null;
            object state = null;
            TimeSpan dueTime = TimeSpan.FromSeconds(100);
            TimeSpan period = TimeSpan.FromSeconds(100);
            player.Setup(x => x.RegisterTimer(It.IsAny<Func<object, Task>>(),
                    It.IsAny<object>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>()))
                .Callback<Func<object, Task>, object, TimeSpan, TimeSpan>((a, b, c, d) =>
                {
                    action = a;
                    state = b;
                    dueTime = c;
                    period = d;
                }).Returns(Mock.Of<IDisposable>());
            await this.player.Object.Play("roar");

            //Act
            string res = await this.player.Object.Play("roar");
            //Assert
            Assert.Equal("Roar is on cooldown", res);
            Assert.NotNull(action);
            Assert.Equal(20, dueTime.TotalSeconds);
            Assert.Equal(-1, period.TotalSeconds);
            Assert.Null(state);
        }""" + bossPresent
      }
        val semanticType: Type = 'bossPresent('roar, boss) =>: 'testAbility('roar, boss)
  }

  @combinator object testNoAbility {
    def apply(boss: String): String = {boss + ""}
    val semanticType: Type = 'bossPresent('none, boss) =>: 'testAbility('none, boss)
  }
  
  def semanticPlayerTarget: Type = {
      adventureGame.getAbility match {
          case AbilityTypes.fireball => 'player('fireball)
          case AbilityTypes.roar => 'player('roar)
          case AbilityTypes.none => 'player('none)
      }
  }

  def semanticPlayerTestTarget: Type = {
      var b: Type = 'variationWithoutBoss;
      val a: Type = adventureGame.getAbility match {
          case AbilityTypes.fireball => 'fireball
          case AbilityTypes.roar => 'roar
          case AbilityTypes.none => 'none
      }
      if (adventureGame.getBoss != BossAbilityTypes.none) {
          b = 'variationWithBoss
      }else {
          b = 'variationWithoutBoss
      }
      'playerTest(a, b)
  }

  //########################################################################################################
  //################################## AdventureSetup ######################################################
  @combinator object AdventureSetup {
      def apply(setup: String): MyResult = {
          val file = MyResult(readFile("Adventure.cs"), "Adventure.cs")
          addArbCode(file, setup, "public class Adventure", '{')
          file
      }
      val semanticType: Type =
        'setup(boss) =>: 'AdventureSetup(boss)
  }

  @combinator object SetupBoss {
      def apply(): String = {
        """
        private async Task MakeBoss(IRoomGrain room)
        {
            var monsterGrain = client.GetGrain<IBossGrain>(666);
            await monsterGrain.SetInfo();
            await monsterGrain.SetRoomGrain(room);
        }

        public async Task Configure(string filename)
        {
            var rand = new Random();

            using (var jsonStream = new JsonTextReader(File.OpenText(filename)))
            {
                var deserializer = new JsonSerializer();
                var data = deserializer.Deserialize<MapInfo>(jsonStream);

                var rooms = new List<IRoomGrain>();
                foreach (var room in data.Rooms)
                {
                    var roomGr = await MakeRoom(room);
                    if (room.Id >= 0)
                        rooms.Add(roomGr);
                }
                foreach (var thing in data.Things)
                {
                    await MakeThing(thing);
                }
                foreach (var monster in data.Monsters)
                {
                    await MakeMonster(monster, rooms[rand.Next(0, rooms.Count)]);
                }
                await MakeBoss(rooms[4]);
            }
        }"""
      }
      val semanticType: Type =
        'setup('variationWithBoss)
  }

  @combinator object SetupNoBoss {
      def apply(): String = {
        """
        public async Task Configure(string filename)
        {
            var rand = new Random();

            using (var jsonStream = new JsonTextReader(File.OpenText(filename)))
            {
                var deserializer = new JsonSerializer();
                var data = deserializer.Deserialize<MapInfo>(jsonStream);

                var rooms = new List<IRoomGrain>();
                foreach (var room in data.Rooms)
                {
                    var roomGr = await MakeRoom(room);
                    if (room.Id >= 0)
                        rooms.Add(roomGr);
                }
                foreach (var thing in data.Things)
                {
                    await MakeThing(thing);
                }
                foreach (var monster in data.Monsters)
                {
                    await MakeMonster(monster, rooms[rand.Next(0, rooms.Count)]);
                }
            }
        }"""
      }
      val semanticType: Type =
        'setup('variationWithoutBoss)
  }

  def semanticAdventureTarget: Type = {
      if (adventureGame.getBoss != BossAbilityTypes.none) {
          return 'AdventureSetup('variationWithBoss)
      }else {
          return 'AdventureSetup('variationWithoutBoss)
      }
  }
  //##############################################################################################
  //################################## BOSS ######################################################
  lazy val bossAbility = Variable("bossAbility")
  lazy val bossAbilityKinding: Kinding =
  Kinding(bossAbility)
    .addOption('heal).addOption('DR)

  @combinator object BossGrain {
      def apply(bossOnActivateAsync: String,
            bossRoomInteracation: String,
            bossAbility: String): MyResult = {
          val file = MyResult(readFile("BossGrain.cs"), "BossGrain.cs")
          addArbCode(file, bossOnActivateAsync, "public class BossGrain", '{')
          addArbCode(file, bossRoomInteracation, "public class BossGrain", '{')
          addArbCode(file, bossAbility, "public class BossGrain", '{')
          file
      }
      val semanticType: Type =
        'BossOnActivateAsync(bossAbility) =>: 'bossRoomInteracation(bossAbility)=>: 'bossAbility(bossAbility) =>: 'boss(bossAbility)
  }

  @combinator object BossGrainEmpty {
      def apply(): MyResult = {
          val file = MyResult("", "BossGrain.cs")
          file
      }
      val semanticType: Type =
        'boss('none)
  }

  @combinator object BossOnActivateAsyncHeal {
      def apply(): String = {
        """
        public override Task OnActivateAsync()
        {
            this.healTimer = RegisterTimer((_) => HealAdds(), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20));
            this.spawnTimer = RegisterTimer((_) => SpawnAdds(this.roomGrain), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(25));
            this.attackTimer = RegisterTimer((_) => Attack(this.roomGrain, this.damage), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
            return base.OnActivateAsync();
        }"""
      }
      val semanticType: Type =
        'BossOnActivateAsync('heal)
  }

  @combinator object BossOnActivateAsyncDR {
      def apply(): String = {
        """
        public override Task OnActivateAsync()
        {
            this.spawnTimer = RegisterTimer((_) => SpawnAdds(this.roomGrain), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(25));
            this.attackTimer = RegisterTimer((_) => Attack(this.roomGrain, this.damage), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
            return base.OnActivateAsync();
        }"""
      }
      val semanticType: Type =
        'BossOnActivateAsync('DR)
  }

  @combinator object bossRoomInteracationHeal {
      def apply(): String = {
        """
        public async Task SpawnAdds(IRoomGrain room)
        {
            List<PlayerInfo> targets = await roomGrain.GetTargetsForMonster();

            if (targets.Count > 0)
            {
                var monsterGrain = GrainFactory.GetGrain<IMonsterGrain>(addCounter, "AdventureGrains.Monster");
                MonsterInfo addInfo = new MonsterInfo();
                addInfo.Id = addCounter;
                addInfo.Name = "one-and-a-half-eyed demon";
                addInfo.KilledBy = new List<long>() {1};
                this.spawnedMonsters.Add(addInfo);
                await monsterGrain.SetInfo(addInfo);
                await monsterGrain.SetRoomGrain(room);
                this.addCounter += 1;
            }

            return;
        }

        public Task UpdateAdds(MonsterInfo mi)
        {
            foreach (MonsterInfo monster in this.spawnedMonsters)
            {
                if (mi.Id == monster.Id)
                {
                    this.spawnedMonsters.Remove(monster);
                    break;
                }
            }

            return Task.CompletedTask;
        }"""
      }
      val semanticType: Type =
        'bossRoomInteracation('heal)
  }

  @combinator object bossRoomInteracationDR {
      def apply(): String = {
        """
        public async Task SpawnAdds(IRoomGrain room)
        {
            List<PlayerInfo> targets = await roomGrain.GetTargetsForMonster();

            if (targets.Count > 0)
            {
                var monsterGrain = GrainFactory.GetGrain<IMonsterGrain>(addCounter, "AdventureGrains.Monster");
                MonsterInfo addInfo = new MonsterInfo();
                addInfo.Id = addCounter;
                addInfo.Name = "one-and-a-half-eyed demon";
                addInfo.KilledBy = new List<long>() {1};
                this.spawnedMonsters.Add(addInfo);
                await monsterGrain.SetInfo(addInfo);
                await monsterGrain.SetRoomGrain(room);
                this.addCounter += 1;
                this.addActive = true; //Damage reduction synthesis
            }

            return;
        }

        public Task UpdateAdds(MonsterInfo mi)
        {
            foreach (MonsterInfo monster in this.spawnedMonsters)
            {
                if (mi.Id == monster.Id)
                {
                    this.spawnedMonsters.Remove(monster);

                    if (this.spawnedMonsters.Count < 1) //Damage reduction synthesis
                    {
                        this.addActive = false;
                    }
                    break;
                }
            }

            return Task.CompletedTask;
        }"""
      }
      val semanticType: Type =
        'bossRoomInteracation('DR)
  }

  @combinator object bossAbilityHeal {
      def apply(): String = {
        """
        public async Task HealAdds()
        {
            if (this.spawnedMonsters.Count > 0)
            {
                foreach (var monster in this.spawnedMonsters)
                {
                    await GrainFactory.GetGrain<IMonsterGrain>(monster.Id).HealMonster(10);
                }
            }
        }"""
      }
      val semanticType: Type =
        'bossAbility('heal)
  }

  @combinator object bossAbilityDR {
      def apply(): String = {""}
      val semanticType: Type =
        'bossAbility('DR)
  }

  def semanticBossTarget: Type = {
      adventureGame.getBoss match {
          case BossAbilityTypes.none => 'boss('none)
          case BossAbilityTypes.heal => 'boss('heal)
          case BossAbilityTypes.DR => 'boss('DR)
      }
  }
  //##############################################################################################

  def forInhabitation: ReflectedRepository[Repository] = {
    ReflectedRepository(
        this,
        classLoader = this.getClass.getClassLoader,
        substitutionSpace = this.abilityKinding.merge(this.bossKinding.merge(this.bossAbilityKinding)))
  }
}
