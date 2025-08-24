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
  
    internal class CardTokens : Service, ICardTokens
    {
        public CardTokens(IHttpClientFactory httpClientFactory, string endpoint, string privateApiKey, Dictionary<string, string> headers) : base(httpClientFactory, endpoint)
        {
            this.restClient = GetRestClient(headers, CONTENT_TYPE_APP_JSON);
        }
        private bool IntDeleteCardToken(RestResponse result)
        {
            bool deleted = false;
            if (result.StatusCode == STATUS_NOCONTENT)
            {
                deleted = true;
            }
            else
            {
                if (isErrorResponse(result.StatusCode))
                    throw new ResponseException(result.StatusCode.ToString(), JsonConvert.DeserializeObject<ErrorResponse>(result.Response));
                else
                    throw new ResponseException(result.StatusCode + " - " + result.Response);
            }

            return deleted;
        }
        public bool DeleteCardToken(string tokenizedCard)
        {
            return IntDeleteCardToken(this.restClient.Delete(String.Format("cardtokens/{0}", tokenizedCard)));
        }
        public async Task<bool> DeleteCardTokenAsync(string tokenizedCard, CancellationToken cancellationToken)
        {
            return IntDeleteCardToken(await this.restClient.DeleteAsync(String.Format("cardtokens/{0}", tokenizedCard), cancellationToken));
        }
    }
}
