using System;
using System.Text;
using Orleans;
using System.Threading.Tasks;
using AdventureGrainInterfaces;

namespace AdventureGrains
{
    public class CloudyWeather : IWeatherEffect
    {
        public async Task<string> WeatherEffect(IRoomGrain room, IPlayerGrain pg, PlayerInfo pi, string desc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(desc);
            sb.AppendLine("It is cloudy!");
            sb.AppendLine(await room.Description(pi));
            
            return sb.ToString();
        }
    }
}