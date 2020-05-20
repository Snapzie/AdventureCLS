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

class Repository(adventureGame: AdventureGame) {
  lazy val ability = Variable("ability")
  lazy val abilityKinding: Kinding =
  Kinding(ability)
    .addOption('fireball).addOption('roar).addOption('none)

  @combinator object PlayerGrain {
      def apply(caseString: String,
            ability: String): MyResult = {
          val file = MyResult(readFile("PlayerGrain.cs"), "PlayerGrain.cs")
          addArbCode(file, caseString, "switch (verb)")
          addArbCode(file, ability, "PlayerGrain")
          file
      }
      val semanticType: Type =
        'case(ability) =>: 'ability(ability) =>: 'player(ability)
  }

  @combinator object PlayerTests {
      def apply(testAbility: String): MyResult = {
          val file = MyResult(readFile("PlayerTests.cs"), "PlayerTests.cs")
          addArbCode(file, testAbility, "public class PlayerMonsterInteraction")
          file
      }
      val semanticType: Type =
        'testAbility(ability) =>: 'playerTest(ability)
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
      def apply(): String = {
        """
        [Fact]
        public async Task FireballDamageTest()
        {
            string text = await this.player.Play("fireball TestMonster");
            Assert.Contains("took 50 damage", text);
        }
        
        [Fact]
        public async Task FireballCooldownTest()
        {
            await this.player.Play("fireball TestMonster");
            string text = await this.player.Play("fireball TestMonster");
            Assert.Equal("Fireball is on cooldown", text);
            
            Thread.Sleep(10010);
            text = await this.player.Play("fireball TestMonster");
            Assert.Equal("TestMonster is dead.", text);
        }

        [Fact]
        public async Task FireballTargetNotFoundTest()
        {
            string text = await this.player.Play("fireball This Is Not A Name");
            Assert.Equal("I can't see This Is Not A Name here. Are you sure?", text);
        }
        
        //White
        [Fact]
        public async Task FireballEmptyStringTest()
        {
            string text = await this.player.Play("fireball ");
            Assert.Equal("Fireball what?", text);
        }"""
      }
        val semanticType: Type = 'testAbility('fireball)
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
      adventureGame.getAbility match {
          case AbilityTypes.fireball => 'playerTest('fireball)
          case AbilityTypes.roar => 'playerTest('roar)
          case AbilityTypes.none => 'playerTest('none)
      }
  }

  def forInhabitation: ReflectedRepository[Repository] = {
    ReflectedRepository(
        this,
        classLoader = this.getClass.getClassLoader,
        substitutionSpace = this.abilityKinding)//.merge(this.damageTakenKinding)
  }
}
