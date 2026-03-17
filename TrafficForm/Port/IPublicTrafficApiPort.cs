using System;
using System.Collections.Generic;
using System.Text;
using TrafficForm.Domain;

namespace TrafficForm.Port
{
    public interface IPublicTrafficApiPort
    {
        Task<List<Location>> findAllVdiLoc();
        Task<List<VdsTrafficResult>> GetTrafficResult(int highwayNo, double minLongitude, double minLatitude, double maxLongitude, double maxLatitude);
    }
}
