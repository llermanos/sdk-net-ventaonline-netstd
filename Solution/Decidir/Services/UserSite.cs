using Decidir.Clients;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Decidir.Model;
using System.Net.Http;
using System.Threading.Tasks;
using Decidir.Services.Contracts;

namespace Decidir.Services
{
    internal class UserSite : Service, IUserSite
    {
        public UserSite(IHttpClientFactory httpClientFactory, string endpoint, string privateApiKey, Dictionary<string, string> headers) : base(httpClientFactory, endpoint)
        {
            this.restClient = GetRestClient(this.endpoint, headers, CONTENT_TYPE_APP_JSON);
        }

        private GetAllCardTokensResponse IntGetAllTokens(RestResponse result)
        {
            GetAllCardTokensResponse tokens = null;
            if (result.StatusCode == STATUS_OK && !String.IsNullOrEmpty(result.Response))
            {
                tokens = JsonConvert.DeserializeObject<GetAllCardTokensResponse>(result.Response);
            }

            return tokens;
        }
        public GetAllCardTokensResponse GetAllTokens(string userId)
        {
            return IntGetAllTokens(this.restClient.Get("usersite", String.Format("/{0}/cardtokens", userId)));
        }
        public async Task<GetAllCardTokensResponse> GetAllTokensAsync(string userId)
        {
            return IntGetAllTokens(await this.restClient.GetAsync("usersite", String.Format("/{0}/cardtokens", userId)));
        }
    }
}
