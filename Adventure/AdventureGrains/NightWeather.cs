using System;
using System.Text;
using Orleans;
using System.Threading.Tasks;
using AdventureGrainInterfaces;

namespace AdventureGrains
{
    public class NightWeather : IWeatherEffect
    {
        public Task<string> WeatherEffect(IRoomGrain room, IPlayerGrain pg, PlayerInfo pi, string desc)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(desc);
            sb.AppendLine("It is dark!");
            sb.AppendLine("It is hard to see anything!");
            return Task.FromResult(sb.ToString());
        }
    }
}