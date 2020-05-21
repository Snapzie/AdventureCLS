using Orleans;
using System.Threading.Tasks;

namespace AdventureGrainInterfaces
{
    public interface IMonsterGrain : IEnemy, IGrainWithGuidKey
    {
        Task SetInfo(MonsterInfo info);
        //================= CHANGES =====================
        Task HealMonster(int heal);
        //===============================================
    }
}
