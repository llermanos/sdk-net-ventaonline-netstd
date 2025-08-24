using Decidir.Clients;
using System.Collections.Generic;
using System.Net.Http;

namespace Decidir.Services
{
    internal abstract class Service
    {
        protected string endpoint;
        protected IRestClient restClient;

        protected const string CONTENT_TYPE_APP_JSON = "application/json";
        protected const string METHOD_POST = "POST";
        protected const string METHOD_GET = "GET";
        protected const int STATUS_OK = 200;
        protected const int STATUS_CREATED = 201;
        protected const int STATUS_ACCEPTED = 202;
        protected const int STATUS_NOCONTENT = 204;
        protected const int STATUS_ERROR = 500;
        protected const string STATUS_CHALLENGE_PENDING = "CHALLENGE PENDING";
        protected const string STATUS_FINGERPRINT_PENDING = "FINGERPRINT PENDING";

        protected readonly IHttpClientFactory _httpClientFactory;   
        public Service(IHttpClientFactory httpClientFactory, string endpoint)
        {
            _httpClientFactory = httpClientFactory; 
            this.endpoint = endpoint;
        }

        protected bool isErrorResponse(int statusCode)
        {
            if (statusCode == 402)
                return false;
            else
                if (statusCode >= 400 && statusCode < 500)
                    return true;
                else
                    return false;
        }

        protected IRestClient GetRestClient(Dictionary<string, string> headers) => GetRestClient(this.endpoint, headers);
        protected IRestClient GetRestClient(Dictionary<string, string> headers, string contenttype) => GetRestClient(this.endpoint, headers, contenttype);

        protected IRestClient GetRestClient(string endpoint, Dictionary<string, string> headers)
        {
            if (_httpClientFactory is null)
            {
                return new RestClient(endpoint, headers);
            }
            return new RestClientHttpFactory(_httpClientFactory, endpoint, headers);
        }
        protected IRestClient GetRestClient(string endpoint, Dictionary<string, string> headers, string contenttype)
        {
            if (_httpClientFactory is null)
            {
                return new RestClient(endpoint, headers, contenttype);
            }
            return new RestClientHttpFactory(_httpClientFactory, endpoint, headers, contenttype);
        }
    }
}
