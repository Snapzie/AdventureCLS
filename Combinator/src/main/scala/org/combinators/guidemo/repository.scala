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

class Repository(adventureGame: AdventureGame) {
  lazy val ability = Variable("ability")
  lazy val abilityKinding: Kinding =
  Kinding(ability)
    .addOption('fireball).addOption('roar).addOption('none)

  lazy val boss = Variable("boss")
  lazy val bossKinding: Kinding =
  Kinding(boss)
    .addOption('variationWithBoss).addOption('variationWithoutBoss)

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

//   @combinator object RoomGrain {
//       def apply(caseString: String,
//             ability: String,
//             damageTaken: String): MyResult = {
//           val file = MyResult(readFile("PlayerGrain.cs"), "PlayerGrain.cs")
//           addArbCode(file, caseString, "switch (verb)")
//           addArbCode(file, ability, "PlayerGrain")
//           addArbCode(file, damageTaken, "if (this.roomGrain.GetPrimaryKey() == room.GetPrimaryKey())")
//           file
//       }
//       val semanticType: Type =
//         'case(ability) =>: 'ability(ability) =>: 'room('weather)
//   }

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
        private bool roarActive = false;

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

  @combinator object testFireballBoss {
      def apply(): String = {
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
        val semanticType: Type = 'bossPresent('fireball, 'variationWithBoss)
  }

  @combinator object testFireballNoBoss {
      def apply(): String = {
            """
            """
      }
        val semanticType: Type = 'bossPresent(ability, 'variationWithoutBoss)
  }

  @combinator object testRoar {
      def apply(): String = {
        """
        [Fact]
        public async Task RoarTest()
        {
            Assert.Equal(100, await this.player.GetHealth());
            string text = await this.player.Play("roar");
            Assert.Equal("Roar has been activated!", text);
            Thread.Sleep(2010);
            Assert.Equal(95, await this.player.GetHealth());
            
            Thread.Sleep(10010);
            Assert.Equal(85, await this.player.GetHealth());
        }
        
        [Fact]
        public async Task RoarCooldownTest()
        {
            string text = await this.player.Play("roar");
            Assert.Equal("Roar has been activated!", text);
            text = await this.player.Play("roar");
            Assert.Equal("Roar is on cooldown", text);

            Thread.Sleep(20010);
            text = await this.player.Play("roar");
            Assert.Equal("Roar has been activated!", text);
        }"""
      }
        val semanticType: Type = 'testAbility('roar)
  }

  @combinator object testNone {
    def apply(): String = {""}
    val semanticType: Type = 'testAbility('none)
  }
  
  def semanticPlayerTarget: Type = {
      adventureGame.getAbility match {
          case AbilityTypes.fireball => 'player('fireball)
          case AbilityTypes.roar => 'player('roar)
          case AbilityTypes.none => 'player('none)
      }
  }

  def semanticPlayerTestTarget: Type = {
      val a: Type = adventureGame.getAbility match {
          case AbilityTypes.fireball => 'fireball
          case AbilityTypes.roar => 'playerTest('roar)
          case AbilityTypes.none => 'playerTest('none)
      }
      if (adventureGame.getWeather.contains(WeatherTypes.Blizzard)) {
          return 'playerTest(a, 'variationWithBoss)
      }
      'playerTest('roar, 'variationWithBoss)
  }

  def forInhabitation: ReflectedRepository[Repository] = {
    ReflectedRepository(
        this,
        classLoader = this.getClass.getClassLoader,
        substitutionSpace = this.abilityKinding.merge(this.bossKinding))//.merge(this.damageTakenKinding)
  }
}
