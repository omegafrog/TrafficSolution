using System;
using System.Collections.Generic;
using System.Text;
using TrafficForm.Domain;

namespace TrafficForm.Port
{
    public interface IPublicTrafficApiPort
    {
        Task<TrafficResult> GetTrafficResult(HighWay highWay);
    }
}
