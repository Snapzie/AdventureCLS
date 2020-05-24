using System.Threading.Tasks;
using Orleans;

namespace AdventureGrainInterfaces
{
    public interface IBossGrain : IEnemy
    {
        Task SpawnAdds(IRoomGrain room);
        Task SetInfo();
        Task UpdateAdds(MonsterInfo mi);
    }
}