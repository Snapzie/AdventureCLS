using AdventureGrainInterfaces;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventureGrains
{
    public class PlayerGrain : Orleans.Grain, IPlayerGrain
    {
        //==================== CHANGES =======================
        private int health = 100; //Change for player classes
        private int damage = 20;
        private bool fireballCD = false;
        private bool roarCD = false;
        private bool roarActive = false;
        //====================================================
        
        IRoomGrain roomGrain; // Current room
        List<Thing> things = new List<Thing>(); // Things that the player is carrying

        bool killed = false;

        PlayerInfo myInfo;

        public override Task OnActivateAsync()
        {
            this.myInfo = new PlayerInfo { Key = this.GetPrimaryKey(), Name = "nobody" };
            return base.OnActivateAsync();
        }
        
        //==================== CHANGES =======================
        public Task<int> GetHealth()
        {
            return Task.FromResult<int>(this.health);
        }
        //====================================================

        async Task IPlayerGrain.Die()
        {
            // Drop everything
            var tasks = new List<Task<string>>();
            foreach (var thing in new List<Thing>(things))
            {
                tasks.Add(this.Drop(thing));
            }
            await Task.WhenAll(tasks);

            // Exit the game
            if (this.roomGrain != null)
            {
                await this.roomGrain.Exit(myInfo);
                this.roomGrain = null;
                killed = true;
            }
        }

        async Task<string> Drop(Thing thing)
        {
            if ( killed )
                return await CheckAlive();

            if (thing != null)
            {
                this.things.Remove(thing);
                await this.roomGrain.Drop(thing);
                return "Okay.";
            }
            else
                return "I don't understand.";
        }

        async Task<string> Take(Thing thing)
        {
            if (killed)
                return await CheckAlive();

            if (thing != null)
            {
                this.things.Add(thing);
                await this.roomGrain.Take(thing);
                return "Okay.";
            }
            else
                return "I don't understand.";
        }


        Task IPlayerGrain.SetName(string name)
        {
            this.myInfo.Name = name;
            return Task.CompletedTask;
        }

        Task IPlayerGrain.SetRoomGrain(IRoomGrain room)
        {
            this.roomGrain = room;
            return room.Enter(myInfo);
        }

        async Task<string> Go(string direction)
        {
            IRoomGrain destination = await this.roomGrain.ExitTo(direction);

            StringBuilder description = new StringBuilder();

            if (destination != null)
            {
                await this.roomGrain.Exit(myInfo);
                await destination.Enter(myInfo);

                this.roomGrain = destination;
                var desc = await destination.Description(myInfo);

                if (desc != null)
                    description.Append(desc);
            }
            else
            {
                description.Append("You cannot go in that direction.");
            }

            if (things.Count > 0)
            {
                description.AppendLine("You are holding the following items:");
                foreach (var thing in things)
                {
                    description.AppendLine(thing.Name);
                }
            }

            return description.ToString();
        }

        async Task<string> CheckAlive()
        {
            if (!killed)
                return null;

            // Go to room '-2', which is the place of no return.
            var room = GrainFactory.GetGrain<IRoomGrain>(-2);
            return await room.Description(myInfo);
        }

        async Task<string> Kill(string target)
        {
            if (things.Count == 0)
                return "With what? Your bare hands?";

            var player = await this.roomGrain.FindPlayer(target);
            if (player != null)
            {
                var weapon = things.Where(t => t.Category == "weapon").FirstOrDefault();
                if (weapon != null)
                {
                    await GrainFactory.GetGrain<IPlayerGrain>(player.Key).Die();
                    return target + " is now dead.";
                }
                return "With what? Your bare hands?";
            }

            var monster = await this.roomGrain.FindMonster(target);
            if (monster != null)
            {
                var weapons = monster.KilledBy.Join(things, id => id, t => t.Id, (id, t) => t);
                if (weapons.Count() > 0)
                {
                    //======================================== CHANGES =============================================
                    return await GrainFactory.GetGrain<IMonsterGrain>(monster.Id).Kill(this.roomGrain, this.damage);
                    //==============================================================================================
                }
                return "With what? Your bare hands?";
            }
            //======================================== CHANGES =============================================
            var boss = await this.roomGrain.GetBoss();
            if (boss != null)
            {
                var weapons = boss.KilledBy.Join(things, id => id, t => t.Id, (id, t) => t);
                if (weapons.Count() > 0)
                {
                    return await GrainFactory.GetGrain<IBossGrain>(boss.Id).Kill(this.roomGrain, this.damage);
                }
                return "With what? Your bare hands?";
            }
            //==============================================================================================
            return "I can't see " + target + " here. Are you sure?";
        }
        
        //=========================== CHANGES ==============================
        public async Task TakeDamage(IRoomGrain room, int damage)
        {
            if (this.roomGrain != null)
            {
                if (this.roomGrain.GetPrimaryKey() == room.GetPrimaryKey())
                {
                    if (roarActive) //TODO: Remove for synthesis
                    {
                        this.health -= (int)(damage * 0.5);
                    }
                    else
                    {
                        this.health -= damage;   
                    }

                    if (this.health <= 0)
                    {
                        await GrainFactory.GetGrain<IPlayerGrain>(this.myInfo.Key).Die();
                    }
                }
            }

            return;
        }

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

            var boss = await this.roomGrain.GetBoss();
            if (boss != null)
            {
                string res = await GrainFactory.GetGrain<IBossGrain>(boss.Id).Kill(this.roomGrain, 50);
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
        }

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
        }

        public async Task WeatherEffect(int effect)
        {
            this.health += effect;
            if (this.health <= 0)
            {
                await GrainFactory.GetGrain<IPlayerGrain>(this.myInfo.Key).Die();
            }

            return;
        }
        //==================================================================

        private string RemoveStopWords(string s)
        {
            string[] stopwords = new string[] { " on ", " the ", " a " };

            StringBuilder sb = new StringBuilder(s);
            foreach (string word in stopwords)
            {
                sb.Replace(word, " ");
            }

            return sb.ToString();
        }

        private Thing FindMyThing(string name)
        {
            return things.Where(x => x.Name == name).FirstOrDefault();
        }

        private string Rest(string[] words)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 1; i < words.Length; i++)
                sb.Append(words[i] + " ");

            return sb.ToString().Trim().ToLower();
        }

        async Task<string> IPlayerGrain.Play(string command)
        {
            Thing thing;
            string target;
            command = RemoveStopWords(command);

            string[] words = command.Split(' ');

            string verb = words[0].ToLower();

            if (killed && verb != "end")
                return await CheckAlive();

            switch (verb)
            {
                case "look":
                    return await this.roomGrain.Description(myInfo);

                case "go":
                    if (words.Length == 1)
                        return "Go where?";
                    return await Go(words[1]);

                case "north":
                case "south":
                case "east":
                case "west":
                    return await Go(verb);

                case "kill":
                    if (words.Length == 1)
                        return "Kill what?";
                    target = command.Substring(verb.Length + 1);
                    if (target == "")
                    {
                        return "Kill what?";
                    }
                    return await Kill(target);

                case "drop":
                    thing = FindMyThing(Rest(words));
                    return await Drop(thing);

                case "take":
                    thing = await roomGrain.FindThing(Rest(words));
                    return await Take(thing);

                case "inv":
                case "inventory":
                    return "You are carrying: " + string.Join(" ", things.Select(x => x.Name));
                
                //======================= CHANGES ============================
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
                    return await Fireball(target);
                
                case "roar":
                    if (words.Length > 1)
                        return "Can not roar others";
                    if (roarCD)
                    {
                        return "Roar is on cooldown";
                    }
                    return await Roar();
                //============================================================

                case "end":
                    return "";
            }
            return "I don't understand.";
        }
    }
}
