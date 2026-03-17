using System;
using System.Collections.Generic;
using System.Text;
using TrafficForm.Domain;
using TrafficForm.Port;

namespace TrafficForm.Adapter
{
    internal class OpenStreetQueryAdapter : IOpenStreetQueryPort
    {
        private OpenStreetDbRepository repository;

        public OpenStreetQueryAdapter(OpenStreetDbRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Dictionary<int, HighWay>> GetAdjacentHighWays(Location location)
        {
            Dictionary<int, HighWay> results = await repository.findAdjacentHighways(location.Latitude, location.Longitude);
            if(results.Count == 0)
            {
                throw new EntityNotFoundException("No adjacent highways found for the given location.");
            }
            return results;
        }

    }
}
