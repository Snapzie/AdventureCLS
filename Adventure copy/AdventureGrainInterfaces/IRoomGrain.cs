using System;
using System.Collections.Generic;
using Orleans;
using System.Threading.Tasks;
using System;

namespace AdventureGrainInterfaces
{
    /// <summary>
    /// A room is any location in a game, including outdoor locations and
    /// spaces that are arguably better described as moist, cold, caverns.
    /// </summary>
    public interface IRoomGrain : IGrainWithIntegerKey
    {
        // Rooms have a textual description
        Task<string> Description(PlayerInfo whoisAsking);
        Task SetInfo(RoomInfo info);
        Task<IRoomGrain> ExitTo(string direction);
        // Players can enter or exit a room
        Task<string> Enter(PlayerInfo player);
        Task Exit(PlayerInfo player);
        // Players can enter or exit a room
        Task Enter(MonsterInfo monster);
        Task Exit(MonsterInfo monster);
        // Things can be dropped or taken from a room
        Task Drop(Thing thing);
        Task Take(Thing thing);
        Task<Thing> FindThing(string name);
        // Players and monsters can be killed, if you have the right weapon.
        Task<PlayerInfo> FindPlayer(string name);
        Task<MonsterInfo> FindMonster(string name);
        //==================== CHANGES =======================
        Task<List<PlayerInfo>> GetTargetsForMonster();
        Task BossEnter(MonsterInfo monster);
        Task<MonsterInfo> GetBoss();
        Task BossExit(MonsterInfo monster);
        //Task<Guid> RoomId();
        //====================================================
    }
}
