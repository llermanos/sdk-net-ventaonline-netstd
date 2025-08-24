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
    internal class CheckoutService : Service, ICheckoutService
    {
        private string PrivateApiKey;
        private IRestClient RestClientCheckout;
        Dictionary<string, string> Headers;
        private string EndPointCheckout;

        public CheckoutService(IHttpClientFactory httpClientFactory, string Endpoint, string PrivateApiKey, Dictionary<string, string> Headers) : base(httpClientFactory, Endpoint)
        {
            this.PrivateApiKey = PrivateApiKey;
            this.EndPointCheckout = Endpoint;
            this.restClient = GetRestClient(this.EndPointCheckout, this.Headers, CONTENT_TYPE_APP_JSON);
            this.Headers = Headers;
        }
        private CheckoutResponse IntCheckoutHash(CheckoutResponse checkoutResponse, RestResponse result)
        {

            Console.WriteLine("RESULTADO DE GENERACION DE LINK: " + result.StatusCode + " " + result.Response);

            if (!String.IsNullOrEmpty(result.Response))
            {
                try
                {

                    checkoutResponse.response = JsonConvert.DeserializeObject<CheckoutGenerateHashResponse>
                           (result.Response);
                }
                catch (JsonReaderException j)
                {
                    Console.WriteLine("ERROR DE CASTEO: " + j.ToString());
                    ErrorResponse ErrorPaymentResponse = new ErrorResponse();
                    ErrorPaymentResponse.code = "502";
                    ErrorPaymentResponse.error_type = "Error en recepción de mensaje";
                    ErrorPaymentResponse.message = "No se pudo leer la respuesta";
                    ErrorPaymentResponse.validation_errors = null;
                    throw new Exception(ErrorPaymentResponse.code);
                }

                if (checkoutResponse != null)
                {

                    checkoutResponse.statusCode = result.StatusCode;
                }

                if (result.StatusCode != STATUS_CREATED)
                {
                    if (result.StatusCode == STATUS_ERROR)
                    {
                        throw new CheckoutResponseException(result.StatusCode + " - " + result.Response, JsonConvert.DeserializeObject<ErrorCheckoutResponse>(result.Response), result.StatusCode);
                    }
                    else
                    {

                        if (isErrorResponse(result.StatusCode))
                        {

                            throw new CheckoutResponseException(result.StatusCode.ToString(), JsonConvert.DeserializeObject<ErrorCheckoutResponse>(result.Response), result.StatusCode);
                        }
                    }

                }
            }
            return checkoutResponse;
        }

        public CheckoutResponse CheckoutHash(CheckoutRequest checkoutRequest)
        {
            CheckoutResponse checkoutResponse = new CheckoutResponse();
            this.Headers["apikey"] = this.PrivateApiKey;
            this.RestClientCheckout = GetRestClient(this.EndPointCheckout, this.Headers, CONTENT_TYPE_APP_JSON);
            RestResponse result = this.RestClientCheckout.Post("payments/link", CheckoutRequest.toJson(checkoutRequest));

            return IntCheckoutHash(checkoutResponse, result);
        }
        public async Task<CheckoutResponse> CheckoutHashAsync(CheckoutRequest checkoutRequest, CancellationToken cancellationToken)
        {
            CheckoutResponse checkoutResponse = new CheckoutResponse();
            this.Headers["apikey"] = this.PrivateApiKey;
            this.RestClientCheckout = GetRestClient(this.EndPointCheckout, this.Headers, CONTENT_TYPE_APP_JSON);
            RestResponse result = await this.RestClientCheckout.PostAsync("payments/link", CheckoutRequest.toJson(checkoutRequest), cancellationToken);

            return IntCheckoutHash(checkoutResponse, result);
        }
    }
}
