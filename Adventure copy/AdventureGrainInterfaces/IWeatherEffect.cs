using System.Threading.Tasks;
using Orleans;

namespace AdventureGrainInterfaces
{
    public interface IWeatherEffect
    {
        Task<string> WeatherEffect(IRoomGrain room, IPlayerGrain pg, PlayerInfo pi, string desc);
    }
}