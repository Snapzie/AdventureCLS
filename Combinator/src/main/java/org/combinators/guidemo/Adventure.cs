using AdventureGrainInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Newtonsoft.Json;

namespace AdventureSetup
{
    public class Adventure
    {
        private IClusterClient client;

        public Adventure(IClusterClient client)
        {
            this.client = client;
        }

        private async Task<IRoomGrain> MakeRoom(RoomInfo data)
        {
            var roomGrain = client.GetGrain<IRoomGrain>(data.Id);
            await roomGrain.SetInfo(data);
            return roomGrain;
        }

        private async Task MakeThing(Thing thing)
        {
            var roomGrain = client.GetGrain<IRoomGrain>(thing.FoundIn);
            await roomGrain.Drop(thing);
        }

        private async Task MakeMonster(MonsterInfo data, IRoomGrain room)
        {
            var monsterGrain = client.GetGrain<IMonsterGrain>(data.Id);
            await monsterGrain.SetInfo(data);
            await monsterGrain.SetRoomGrain(room);
        }
    }
}
