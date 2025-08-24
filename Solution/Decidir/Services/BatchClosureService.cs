using Decidir.Clients;
using Decidir.Exceptions;
using Decidir.Model;
using Decidir.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Decidir.Services
{
    internal class BatchClosure : Service, IBatchClosure
    {
        public BatchClosure(IHttpClientFactory httpClientFactory, String endpoint, String privateApiKey, String validateApiKey = null, String merchant = null, string request_host = null, string publicApiKey = null) : base(httpClientFactory, endpoint)
        {

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("apikey", privateApiKey);
            headers.Add("Cache-Control", "no-cache");

            this.restClient = GetRestClient(headers, CONTENT_TYPE_APP_JSON);
        }

        private BatchClosureResponse IntBatchClosureActive(RestResponse result)
        {
            BatchClosureResponse refund = null;
            if (result.StatusCode == STATUS_CREATED && !String.IsNullOrEmpty(result.Response))
            {
                refund = JsonConvert.DeserializeObject<BatchClosureResponse>(result.Response);
            }
            else
            {
                throw new ResponseException(result.StatusCode + " - " + result.Response);
            }

            return refund;

        }

        public BatchClosureResponse BatchClosureActive(String batchClosure)
        {
            return IntBatchClosureActive(this.restClient.Post(String.Format("closures/batchclosure"), batchClosure));

        }
        public async Task<BatchClosureResponse> BatchClosureActiveAsync(string batchClosure, CancellationToken cancellationToken)
        {
            return IntBatchClosureActive(await this.restClient.PostAsync(String.Format("closures/batchclosure"), batchClosure, cancellationToken));        
        }
    }
}