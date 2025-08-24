using Decidir.Clients;
using Decidir.Model;
using Decidir.Exceptions;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Decidir.Services.Contracts;
using System.Threading;

namespace Decidir.Services
{
   
    internal class HealthCheck : Service, IHealthCheck
    {
        public HealthCheck(IHttpClientFactory httpClientFactory, string endpoint, Dictionary<string, string> headers) : base(httpClientFactory, endpoint)
        {
            this.restClient = GetRestClient(headers, CONTENT_TYPE_APP_JSON);
        }

        private HealthCheckResponse IntExecute(RestResponse result)
        {
            if (result.StatusCode == STATUS_OK && !String.IsNullOrEmpty(result.Response))
            {
                return HealthCheckResponse.toHealthCheckResponse(result.Response);
            }
            else
            {
                if (isErrorResponse(result.StatusCode))
                    throw new ResponseException(result.StatusCode.ToString(), JsonConvert.DeserializeObject<ErrorResponse>(result.Response));
                else
                    throw new ResponseException(result.StatusCode + " - " + result.Response);
            }

        }
        public HealthCheckResponse Execute()
        {
            return IntExecute(this.restClient.Get("healthcheck", ""));
        }
        public async Task<HealthCheckResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            return IntExecute(await this.restClient.GetAsync("healthcheck", "", cancellationToken));
        }
    }
}
