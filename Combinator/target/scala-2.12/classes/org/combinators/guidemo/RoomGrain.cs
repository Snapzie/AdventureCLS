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
        string description;
        
        //=================================== CHANGES ===========================================
        //activeWeather and WeatherTypes list
        private Random rand = new Random(0); // For testing purposes
        private MonsterInfo boss = null;
        //=======================================================================================

        List<PlayerInfo> players = new List<PlayerInfo>();
        List<MonsterInfo> monsters = new List<MonsterInfo>();
        List<Thing> things = new List<Thing>();

        Dictionary<string, IRoomGrain> exits = new Dictionary<string, IRoomGrain>();

        async Task IRoomGrain.Enter(PlayerInfo player)
        {
            players.RemoveAll(x => x.Key == player.Key);
            players.Add(player);
            //=================================== CHANGES ===========================================
            //BlizzardSunny weather effect implementation
            //=======================================================================================
            return;
        }

        Task IRoomGrain.Exit(PlayerInfo player)
        {
            players.RemoveAll(x => x.Key == player.Key);
            return Task.CompletedTask;
        }

        Task IRoomGrain.Enter(MonsterInfo monster)
        {
            monsters.RemoveAll(x => x.Id == monster.Id);
            monsters.Add(monster);
            return Task.CompletedTask;
        }
        
        //================= CHANGES ============================
        Task IRoomGrain.BossEnter(MonsterInfo monster)
        {
            monsters.RemoveAll(x => x.Id == monster.Id);
            this.boss = monster;
            return Task.CompletedTask;
        }
        
        Task<MonsterInfo> IRoomGrain.GetBoss()
        {
            return Task.FromResult(this.boss);
        }
        
        Task IRoomGrain.BossExit(MonsterInfo monster)
        {
            this.boss = null;
            return Task.CompletedTask;
        }
        //======================================================

        async Task IRoomGrain.Exit(MonsterInfo monster)
        {
            monsters.RemoveAll(x => x.Id == monster.Id);
            if (this.monsters.Count > 0)
            {
                await GrainFactory.GetGrain<IBossGrain>(this.boss.Id).SetAddActive();
            }

            return;
        }

        Task IRoomGrain.Drop(Thing thing)
        {
            things.RemoveAll(x => x.Id == thing.Id);
            things.Add(thing);
            return Task.CompletedTask;
        }

        Task IRoomGrain.Take(Thing thing)
        {
            things.RemoveAll(x => x.Name == thing.Name);
            return Task.CompletedTask;
        }

        Task IRoomGrain.SetInfo(RoomInfo info)
        {
            this.description = info.Description;

            foreach (var kv in info.Directions)
            {
                this.exits[kv.Key] = GrainFactory.GetGrain<IRoomGrain>(kv.Value);
            }
            return Task.CompletedTask;
        }

        Task<Thing> IRoomGrain.FindThing(string name)
        {
            return Task.FromResult(things.Where(x => x.Name == name).FirstOrDefault());
        }

        Task<PlayerInfo> IRoomGrain.FindPlayer(string name)
        {
            name = name.ToLower();
            return Task.FromResult(players.Where(x => x.Name.ToLower().Contains(name)).FirstOrDefault());
        }

        Task<MonsterInfo> IRoomGrain.FindMonster(string name)
        {
            name = name.ToLower();
            return Task.FromResult(monsters.Where(x => x.Name.ToLower().Contains(name)).FirstOrDefault());
        }
        
        //==================== CHANGES =======================
        public Task<List<PlayerInfo>> GetTargetsForMonster()
        {
            return Task.FromResult(players);
        }
        
        public Task<List<MonsterInfo>> GetMonsters()
        {
            return Task.FromResult(monsters);
        }
        //====================================================

        async Task<string> IRoomGrain.Description(PlayerInfo whoisAsking)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(this.description);
            //========================= CHANGES ======================================
            //Description implementation
                //Switch case for weather types
                //Night check
            //========================================================================

            sb.AppendLine($"Your health is: {await GrainFactory.GetGrain<IPlayerGrain>(whoisAsking.Key).GetHealth()}");

            return Task.FromResult(sb.ToString()).Result;
        }

        Task<IRoomGrain> IRoomGrain.ExitTo(string direction)
        {
            return Task.FromResult((exits.ContainsKey(direction)) ? exits[direction] : null);
        }
    }
}
