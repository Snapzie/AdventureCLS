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
        private Random rand = new Random();
        private IDisposable attackTimer;
        private IDisposable spawnTimer;
        private IDisposable healTimer;
        private IRoomGrain roomGrain;
        private int addCounter = 100;
        private bool addActive = false;
        private MonsterInfo monsterInfo = new MonsterInfo();
        private List<MonsterInfo> spawnedMonsters = new List<MonsterInfo>();


        public new virtual IGrainFactory GrainFactory
        {
            get { return base.GrainFactory; }
        }
        
        public Task SetInfo()
        {
            this.monsterInfo.Id = this.GetPrimaryKeyLong();
            this.monsterInfo.Name = "Patches the one-eyed demon";
            this.monsterInfo.KilledBy = new List<long>() {1};
            
            return Task.CompletedTask;
        }
        
        public async Task SetRoomGrain(IRoomGrain room)
        {
            if (this.roomGrain != null)
                await this.roomGrain.Exit(this.monsterInfo);
            this.roomGrain = room;
            await this.roomGrain.BossEnter(this.monsterInfo);
        }

        public Task<string> Kill(IRoomGrain room, int damage)
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