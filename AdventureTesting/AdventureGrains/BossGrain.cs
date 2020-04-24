using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AdventureGrainInterfaces;
using Orleans;

namespace AdventureGrains
{
    public class BossGrain : Orleans.Grain, IBossGrain
    {
        private int health = 200;
        private int damage = 2;
        Random rand = new Random();
        private IDisposable attackTimer;
        private IDisposable spawnTimer;
        private IDisposable healTimer;
        private IRoomGrain roomGrain;
        private int addCounter = 100;
        private bool addActive = false;
        MonsterInfo monsterInfo = new MonsterInfo();
        
        public override Task OnActivateAsync()
        {
            this.healTimer = RegisterTimer((_) => HealAdds(), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20));
            this.spawnTimer = RegisterTimer((_) => SpawnAdds(this.roomGrain), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(25));
            this.attackTimer = RegisterTimer((_) => Attack(this.roomGrain, this.damage), null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
            return base.OnActivateAsync();
        }

        public Task SetAddActive()
        {
            this.addActive = false;
            return Task.CompletedTask;
        }

        public Task SetInfo()
        {
            this.monsterInfo.Id = this.GetPrimaryKeyLong();
            this.monsterInfo.Name = "Patches the one-eyed demon";
            this.monsterInfo.KilledBy = new List<long>() {1};
            
            return Task.CompletedTask;
        }
        
        async Task IEnemy.SetRoomGrain(IRoomGrain room)
        {
            if (this.roomGrain != null)
                await this.roomGrain.Exit(this.monsterInfo);
            this.roomGrain = room;
            await this.roomGrain.BossEnter(this.monsterInfo);
        }

        public async Task SpawnAdds(IRoomGrain room)
        {
            List<PlayerInfo> targets = await roomGrain.GetTargetsForMonster();

            if (targets.Count > 0)
            {
                var monsterGrain = GrainFactory.GetGrain<IMonsterGrain>(addCounter);
                MonsterInfo addInfo = new MonsterInfo();
                addInfo.Id = addCounter;
                addInfo.Name = "one-and-a-half-eyed demon";
                addInfo.KilledBy = new List<long>() {1};
                await monsterGrain.SetInfo(addInfo);
                await monsterGrain.SetRoomGrain(room);
                this.addCounter += 1;
                this.addActive = true;
            }

            return;
        }

        public Task AddBuff(IRoomGrain room)
        {
            throw new System.NotImplementedException();
        }

        public async Task HealAdds()
        {
            List<MonsterInfo> targets = await this.roomGrain.GetMonsters();

            if (targets.Count > 0)
            {
                foreach (var monster in targets)
                {
                    if (monster.Id != this.monsterInfo.Id)
                    {
                        await GrainFactory.GetGrain<IMonsterGrain>(monster.Id).HealMonster(10);   
                    }
                }
            }
        }

        Task<string> IEnemy.Kill(IRoomGrain room, int damage)
        {
            if (this.roomGrain != null)
            {
                if (addActive)
                {
                    this.health -= (int)(damage * 0.5);
                    damage = (int)(damage * 0.5);
                }
                else
                {
                    this.health -= damage;
                }
                
                if (this.health <= 0)
                {
                    this.attackTimer?.Dispose();
                    this.spawnTimer?.Dispose();
                    this.healTimer?.Dispose();
                    return this.roomGrain.BossExit(this.monsterInfo).ContinueWith(t => monsterInfo.Name + " has been slain!");
                }
                else
                {
                    return Task.FromResult(monsterInfo.Name + $" took {damage.ToString()} damage. He now has {this.health} health left!");
                }
            }
            return Task.FromResult(monsterInfo.Name + " is already dead. You were too slow and someone else got to him!");
        }

        public async Task Attack(IRoomGrain room, int damage)
        {
            List<PlayerInfo> targets = await roomGrain.GetTargetsForMonster();

            if (targets.Count > 0)
            {
                int num = rand.Next(0, targets.Count);
                await GrainFactory.GetGrain<IPlayerGrain>(targets[num].Key).TakeDamage(room, damage);   
            }
            return;
        }
    }
}