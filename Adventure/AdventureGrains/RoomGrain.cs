using System;
using AdventureGrainInterfaces;
using Orleans;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventureGrains
{
    public class RoomGrain : Grain, IRoomGrain
    {
        public Random rand = new Random(0); // For testing purposes

        private string description;
        //=================================== CHANGES ===========================================
        private IWeatherEffect activeWeather;
        private MonsterInfo boss = null;
        //=======================================================================================

        List<PlayerInfo> players = new List<PlayerInfo>();
        List<MonsterInfo> monsters = new List<MonsterInfo>();
        List<Thing> things = new List<Thing>();

        Dictionary<string, IRoomGrain> exits = new Dictionary<string, IRoomGrain>();
        
        //==================== CHANGES =======================
        public new virtual IGrainFactory GrainFactory
        {
            get { return base.GrainFactory; }
        }

//        public Task<Guid> RoomId()
//        {
//            return Task.FromResult(this.GetPrimaryKey());
//        }
        //====================================================

        public async Task<string> Enter(PlayerInfo player)
        {
            players.RemoveAll(x => x.Key == player.Key);
            players.Add(player);
            //=================================== CHANGES ===========================================
            int num = rand.Next(0, 4);
            switch (num)
            {
                case 0:
                    activeWeather = new CloudyWeather();
                    break;
                case 1:
                    activeWeather = new SunnyWeather();
                    break;
                case 2:
                    activeWeather = new BlizzardWeather();
                    break;
                case 3:
                    activeWeather = new NightWeather();
                    break;
            }
            IPlayerGrain playerGrain = GrainFactory.GetGrain<IPlayerGrain>(player.Key, "AdventureGrains.Player");
            return await activeWeather.WeatherEffect(this, playerGrain, player, this.description);
            //=======================================================================================
        }

        public Task Exit(PlayerInfo player)
        {
            players.RemoveAll(x => x.Key == player.Key);
            return Task.CompletedTask;
        }

        public Task Enter(MonsterInfo monster)
        {
            monsters.RemoveAll(x => x.Id == monster.Id);
            monsters.Add(monster);
            return Task.CompletedTask;
        }
        
        //================= CHANGES ============================
        public Task BossEnter(MonsterInfo monster)
        {
            monsters.RemoveAll(x => x.Id == monster.Id);
            this.boss = monster;
            return Task.CompletedTask;
        }
        
        public Task<MonsterInfo> GetBoss()
        {
            return Task.FromResult(this.boss);
        }
        
        public Task BossExit(MonsterInfo monster)
        {
            this.boss = null;
            return Task.CompletedTask;
        }
        //======================================================

        public async Task Exit(MonsterInfo monster)
        {
            monsters.RemoveAll(x => x.Id == monster.Id);
            //================================== CHANGES ============================
            if (this.boss != null)
            {
                await GrainFactory.GetGrain<IBossGrain>(this.boss.Id).UpdateAdds(monster);
            }
            //=======================================================================

            return;
        }

        public Task Drop(Thing thing)
        {
            things.RemoveAll(x => x.Id == thing.Id);
            things.Add(thing);
            return Task.CompletedTask;
        }

        public Task Take(Thing thing)
        {
            things.RemoveAll(x => x.Name == thing.Name);
            return Task.CompletedTask;
        }

        public Task SetInfo(RoomInfo info)
        {
            this.description = info.Description;

            foreach (var kv in info.Directions)
            {
                this.exits[kv.Key] = GrainFactory.GetGrain<IRoomGrain>(kv.Value, "AdventureGrains.Room");
            }
            return Task.CompletedTask;
        }

        public Task<Thing> FindThing(string name)
        {
            return Task.FromResult(things.Where(x => x.Name == name).FirstOrDefault());
        }

        public Task<PlayerInfo> FindPlayer(string name)
        {
            name = name.ToLower();
            return Task.FromResult(players.Where(x => x.Name.ToLower().Contains(name)).FirstOrDefault());
        }

        public Task<MonsterInfo> FindMonster(string name)
        {
            name = name.ToLower();
            return Task.FromResult(monsters.Where(x => x.Name.ToLower().Contains(name)).FirstOrDefault());
        }
        
        //==================== CHANGES =======================
        public Task<List<PlayerInfo>> GetTargetsForMonster()
        {
            return Task.FromResult(players);
        }
        //====================================================

        public async Task<string> Description(PlayerInfo whoisAsking)
        {
            //================================== CHANGES ============================
            StringBuilder sb = new StringBuilder();

            if (things.Count > 0)
            {
                sb.AppendLine("The following things are present:");
                foreach (var thing in things)
                {
                    sb.Append("  ").AppendLine(thing.Name);
                }
            }
            
            var others = players.Where(pi => pi.Key != whoisAsking.Key).ToArray();
            if (others.Length > 0 || monsters.Count > 0 || this.boss != null) //Boss Synthesis
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

                if (this.boss != null)
                {
                    sb.Append("  ").AppendLine(this.boss.Name);
                }
            }
            //=======================================================================
            sb.AppendLine($"Your health is: {await GrainFactory.GetGrain<IPlayerGrain>(whoisAsking.Key, "AdventureGrains.Player").GetHealth()}");

            return await Task.FromResult(sb.ToString());
        }

        public Task<IRoomGrain> ExitTo(string direction)
        {
            return Task.FromResult((exits.ContainsKey(direction)) ? exits[direction] : null);
        }
    }
}
