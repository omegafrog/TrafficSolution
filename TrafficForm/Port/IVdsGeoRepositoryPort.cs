using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrafficForm.Domain;

namespace TrafficForm.Port
{
    public interface IVdsGeoRepositoryPort
    {
        Task<Dictionary<string, Tuple<double, double>>> findVdsIdIn(
            int highwayNo,
            double minLatitude,
            double minLongitude,
            double maxLatitude,
            double maxLongitude);

        Task<Dictionary<string, List<Location>>> findResponsibilitySegments(
            int highwayNo,
            IEnumerable<string> vdsIds);

        Task<List<Location>> findAllVdsLoc();
    }
}
