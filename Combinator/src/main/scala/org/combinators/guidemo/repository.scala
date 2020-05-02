package org.combinators.guidemo

import java.net.URL

import org.combinators.cls.interpreter.{combinator, ReflectedRepository}
import org.combinators.cls.types.{Kinding, Type, Variable}
import org.combinators.cls.types.syntax._
import org.combinators.cls.git.{EmptyInhabitationBatchJobResults, Results, ResultLocation}
import org.combinators.templating.twirl.Java

import org.combinators.guidemo.Helpers._

import org.combinators.guidemo.domain.{AdventureGame, AbilityTypes, WeatherTypes}
import org.combinators.templating.persistable.Persistable

class Repository(adventureGame: AdventureGame) {
  lazy val ability = Variable("ability")
  lazy val abilityKinding: Kinding =
  Kinding(ability)
    .addOption('fireball).addOption('roar).addOption('none)

  lazy val damageTaken = Variable("damageTaken")
  lazy val damageTakenKinding: Kinding =
  Kinding(damageTaken)
    .addOption('damageTakenRoar).addOption('damageTakenStandard)

  lazy val weather = Variable("weather")
  lazy val weatherKinding: Kinding =
  Kinding(weather)
    .addOption('noWeather).addOption('weather)

  @combinator object PlayerGrain {
      def apply(caseString: String,
            ability: String,
            damageTaken: String): MyResult = {
          val file = MyResult(readFile("PlayerGrain.cs"), "PlayerGrain.cs")
          addArbCode(file, caseString, "switch (verb)", '{')
          addArbCode(file, ability, "PlayerGrain", '{')
          addArbCode(file, damageTaken, "if (this.roomGrain.GetPrimaryKey() == room.GetPrimaryKey())", '{')
          file
      }
      val semanticType: Type =
        'case(ability) =>: 'ability(ability) =>: 'damageTaken(damageTaken) =>: 'player(ability, damageTaken)
  }

  @combinator object PlayerTests {
      def apply(testAbility: String): MyResult = {
          val file = MyResult(readFile("PlayerTests.cs"), "PlayerTests.cs")
          addArbCode(file, testAbility, "public class PlayerMonsterInteraction", '{')
          file
      }
      val semanticType: Type =
        'testAbility(ability) =>: 'playerTest(ability)
  }

  @combinator object RoomGrain {
      def apply(fieldSetup: String,
            blizzardSunnyEffect: String,
            roomDescription: String): MyResult = {
          val file = MyResult(readFile("RoomGrain.cs"), "RoomGrain.cs")
          addArbCode(file, fieldSetup, "public class RoomGrain", '{')
          addArbCode(file, blizzardSunnyEffect, "players.Add(player)", ';')
          addArbCode(file, roomDescription, "sb.AppendLine(this.description)", ';')
          file
      }
      val semanticType: Type =
        'fieldSetup(weather) =>: 'blizzardSunnyEffect =>: 'roomDescription =>: 'room(weather)
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
                await GrainFactory.GetGrain<IPlayerGrain>(player.Key).TakeDamage(this.roomGrain, 10);
                return $"{target} took 50 damage and now has {await GrainFactory.GetGrain<IPlayerGrain>(player.Key).GetHealth()} left!";
            }

            var monster = await this.roomGrain.FindMonster(target);
            if (monster != null)
            {
                string res = await GrainFactory.GetGrain<IMonsterGrain>(monster.Id).Kill(this.roomGrain, 50);
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
                    if (words.size == 1)
                        return "Fireball what?";
                    if (fireballCD)
                    {
                        return "Fireball is on cooldown";
                    }
                    target = command.Substring(verb.size + 1);
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
                    if (words.size > 1)
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

  @combinator object damageTakenStandard {
      def apply(): String = {
          """
                    this.health -= damage;"""
      }
        val semanticType: Type = 'damageTaken('damageTakenStandard)
  }

  @combinator object damageTakenRoar {
      def apply(): String = {
          """
                    if (roarActive)
                    {
                        this.health -= (int)(damage * 0.5);
                    }
                    else
                    {
                        this.health -= damage;   
                    }"""
      }
        val semanticType: Type = 'damageTaken('damageTakenRoar)
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

  @combinator object fieldSetupWeather {
      def apply(): String = {
        val weathers = adventureGame.getWeather
        val weatherSize = weathers.size()
        var weatherString = 
        """
        private WeatherTypes activeWeather;
        List<WeatherTypes> weathers = new List<WeatherTypes>() {"""
        if (weatherSize == 1){
            weathers.get(weatherSize - 1) match {
                case WeatherTypes.Blizzard => weatherString += "WeatherTypes.Blizzard};"
                case WeatherTypes.Sunny => weatherString += "WeatherTypes.Sunny};"
                case WeatherTypes.Night => weatherString += "WeatherTypes.Night};"
                case WeatherTypes.Cloudy => weatherString += "WeatherTypes.Cloudy};"
            }
        } else {
            for (i <- 0 to weatherSize - 2) {
                weathers.get(i) match {
                    case WeatherTypes.Blizzard => weatherString += "WeatherTypes.Blizzard, "
                    case WeatherTypes.Sunny => weatherString += "WeatherTypes.Sunny, "
                    case WeatherTypes.Night => weatherString += "WeatherTypes.Night, "
                    case WeatherTypes.Cloudy => weatherString += "WeatherTypes.Cloudy, "
                }
            }
            weathers.get(weatherSize - 1) match {
                case WeatherTypes.Blizzard => weatherString += "WeatherTypes.Blizzard};"
                case WeatherTypes.Sunny => weatherString += "WeatherTypes.Sunny};"
                case WeatherTypes.Night => weatherString += "WeatherTypes.Night};"
                case WeatherTypes.Cloudy => weatherString += "WeatherTypes.Cloudy};"
            }
        }
        weatherString
      }
        val semanticType: Type = 'fieldSetup('weather)
  }

  @combinator object fieldSetupNoWeather {
      def apply(): String = {""}
      val semanticType: Type = 'fieldSetup('noWeather)
  }

  @combinator object blizzardSunnyEffect {
      def apply(): String = {
        if (adventureGame.getWeather.contains(WeatherTypes.Blizzard) || adventureGame.getWeather.contains(WeatherTypes.Sunny)){
        """
            activeWeather = weathers[rand.Next(0, weathers.Count)];
            switch (activeWeather)
            {
                case WeatherTypes.Blizzard:
                    await GrainFactory.GetGrain<IPlayerGrain>(player.Key).TakeDamage(this, 5);
                    break;
                case WeatherTypes.Sunny:
                    await GrainFactory.GetGrain<IPlayerGrain>(player.Key).TakeDamage(this, -10); //Healing the player
                    break;
            }"""
        }else {""}
      }
        val semanticType: Type = 'blizzardSunnyEffect
  }

  @combinator object roomDescription {
      def apply(): String = {
        val weathers = adventureGame.getWeather
        var descString = ""
        
        if (weathers.size() > 0){
            descString += 
            """
            switch (activeWeather)
                {
            """
            for (i <- 0 to weathers.size() - 1) {
                weathers.get(i) match {
                    case WeatherTypes.Blizzard => descString += 
                        """
                            case WeatherTypes.Blizzard:
                                sb.AppendLine("It is hailing!");
                                break;
                        """
                    case WeatherTypes.Night => descString += 
                        """
                            case WeatherTypes.Night:
                                sb.AppendLine("It is dark!");
                                break;
                        """
                    case WeatherTypes.Sunny => descString += 
                        """
                            case WeatherTypes.Sunny:
                                sb.AppendLine("It is sunny!");
                                break;
                        """
                    case WeatherTypes.Cloudy => descString += 
                        """
                            case WeatherTypes.Cloudy:
                                sb.AppendLine("It is cloudy!");
                                break;
                        """
                }
            }
            descString += "}"
        }

        if (weathers.contains(WeatherTypes.Night)){
            descString += 
            """
                if (activeWeather != WeatherTypes.Night)
                {
                    if (things.Count > 0)
                    {
                        sb.AppendLine("The following things are present:");
                        foreach (var thing in things)
                        {
                            sb.Append("  ").AppendLine(thing.Name);
                        }
                    }
                    
                    
                    var others = players.Where(pi => pi.Key != whoisAsking.Key).ToArray();
                    if (others.Length > 0 || monsters.Count > 0 || this.boss != null) //Boss Stuff
                    {
                        sb.AppendLine("Beware! These guys are in the room with you:");
                        if (others.Length > 0)
                            foreach (var player in others)
                            {
                                sb.Append("  ").AppendLine(player.Name);
                            }
                        if (monsters.Count > 0)
                            foreach (var monster in monsters)
                            {
                                sb.Append("  ").AppendLine(monster.Name);
                            }
                        
                        //Boss stuff    
                        // if (this.boss != null)
                        // {
                        //     sb.Append("  ").AppendLine(this.boss.Name);
                        // }
                    }
                }
                else
                {
                    sb.AppendLine("It is too dark to see anything!");
                }"""
        }else {
            descString += 
            """
                if (things.Count > 0)
                    {
                        sb.AppendLine("The following things are present:");
                        foreach (var thing in things)
                        {
                            sb.Append("  ").AppendLine(thing.Name);
                        }
                    }
                    
                    
                    var others = players.Where(pi => pi.Key != whoisAsking.Key).ToArray();
                    if (others.Length > 0 || monsters.Count > 0 || this.boss != null) //Boss Stuff
                    {
                        sb.AppendLine("Beware! These guys are in the room with you:");
                        if (others.Length > 0)
                            foreach (var player in others)
                            {
                                sb.Append("  ").AppendLine(player.Name);
                            }
                        if (monsters.Count > 0)
                            foreach (var monster in monsters)
                            {
                                sb.Append("  ").AppendLine(monster.Name);
                            }
                        
                        //Boss stuff    
                        // if (this.boss != null)
                        // {
                        //     sb.Append("  ").AppendLine(this.boss.Name);
                        // }
                    }"""
        }
        descString
      }
        val semanticType: Type = 'roomDescription
  }

  @combinator object testNone {
    def apply(): String = {""}
    val semanticType: Type = 'testAbility('none)
  }
  
  def semanticPlayerTarget: Type = {
      adventureGame.getAbility match {
          case AbilityTypes.fireball => 'player('fireball, 'damageTakenStandard)
          case AbilityTypes.roar => 'player('roar, 'damageTakenRoar)
          case AbilityTypes.none => 'player('none, 'damageTakenStandard)
      }
  }

  def semanticPlayerTestTarget: Type = {
      adventureGame.getAbility match {
          case AbilityTypes.fireball => 'playerTest('fireball)
          case AbilityTypes.roar => 'playerTest('roar)
          case AbilityTypes.none => 'playerTest('none)
      }
  }

  def semanticRoomTarget: Type = {
      val weathers = adventureGame.getWeather
      if (weathers.size == 0) {
          'room('noWeather)
      }
      else {
          'room('weather)
      }
  } 

  def semanticRoomTestTarget: Type = {
      val weathers = adventureGame.getWeather
      if (weathers.size == 0) {
          'room('noWeather)
      }
      else {
          'room('weather)
      }
  }

  def forInhabitation: ReflectedRepository[Repository] = {
    ReflectedRepository(
        this,
        classLoader = this.getClass.getClassLoader,
        substitutionSpace = this.abilityKinding.merge(this.damageTakenKinding.merge(this.weatherKinding)))
  }
}
