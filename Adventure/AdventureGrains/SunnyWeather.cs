using System;
using System.Text;
using Orleans;
using System.Threading.Tasks;
using AdventureGrainInterfaces;

namespace AdventureGrains
{
    public class SunnyWeather : IWeatherEffect
    {
        public async Task<string> WeatherEffect(IRoomGrain room, IPlayerGrain pg, PlayerInfo pi, string desc)
        {
            await pg.TakeDamage(room, -10);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(desc);
            sb.AppendLine("It is sunny!");
            sb.AppendLine(await room.Description(pi));
            
            return Task.FromResult(sb.ToString()).Result;
        }
    }
}