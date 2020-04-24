using System.Threading.Tasks;
using Orleans;

namespace AdventureGrainInterfaces
{
    public interface IBossGrain : IEnemy
    {
        Task SpawnAdds(IRoomGrain room);
        Task AddBuff(IRoomGrain room);
        Task HealAdds();
        Task SetInfo();
        Task SetAddActive();
    }
}