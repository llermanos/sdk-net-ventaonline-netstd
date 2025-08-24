using Decidir.Clients;
using Decidir.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Decidir.Services.Contracts
{
    internal interface IHealthCheck
    {
        HealthCheckResponse Execute();
        Task<HealthCheckResponse> ExecuteAsync();
    }
}
