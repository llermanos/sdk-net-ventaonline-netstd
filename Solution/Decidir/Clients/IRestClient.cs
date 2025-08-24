using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Decidir.Clients
{
    internal interface IRestClient
    {
        void AddContentType(string contentType);
        void AddHeaders(Dictionary<string, string> headers);
        RestResponse Delete(string url);
        Task<RestResponse> DeleteAsync(string url, CancellationToken cancellationToken = default);
        RestResponse Get(string url, string data);
        Task<RestResponse> GetAsync(string url, string data, CancellationToken cancellationToken = default);
        RestResponse Post(string url, string data);
        Task<RestResponse> PostAsync(string url, string data, CancellationToken cancellationToken = default);
        RestResponse Put(string url, string data = null);
        Task<RestResponse> PutAsync(string url, string data = null, CancellationToken cancellationToken = default);
    }
}
