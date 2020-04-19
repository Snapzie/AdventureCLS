using System.Threading.Tasks;
using Orleans;

namespace AdventureGrainInterfaces
{
    public interface IEnemy : IGrainWithIntegerKey
    {
        Task<string> Kill(IRoomGrain room, int damage);
        Task SetRoomGrain(IRoomGrain room);
        Task Attack(IRoomGrain room, int damage);
    }
}