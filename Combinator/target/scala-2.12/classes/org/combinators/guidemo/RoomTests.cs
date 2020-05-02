using System;
using System.Collections.Generic;
using AdventureGrainInterfaces;
using Orleans.TestingHost;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests
{
    [Collection(ClusterCollection.Name)]
    public class RoomEffects
    {
        private readonly TestCluster _cluster;
        private IRoomGrain room;
        private PlayerInfo playerInfo = new PlayerInfo();

        public RoomEffects(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
            
            //Room Setup
            int num = new Random().Next();
            this.room = _cluster.GrainFactory.GetGrain<IRoomGrain>(num);
            RoomInfo ri = new RoomInfo();
            ri.Description = "This is a test room";
            ri.Directions = new Dictionary<string, long>();
            ri.Id = num;
            ri.Name = "TestRoom";
            this.room.SetInfo(ri).Wait();

            //Player Setup
            this.playerInfo.Key = new Guid();
            this.playerInfo.Name = "TestPlayer";
        }
        
        //RoomEffect Test
    }
}