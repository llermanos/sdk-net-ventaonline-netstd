using Decidir.Clients;
using Decidir.Exceptions;
using Decidir.Model;
using Decidir.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Decidir.Services
{
    internal sealed class Payments : Service, IPayments
    {
        private readonly string privateApiKey;
        private readonly string publicApiKey;
        private readonly string validateApiKey;
        private readonly string merchant;
        private readonly string request_host;
        private readonly Dictionary<string, string> headers;
        private readonly string endpointInternalToken;

        public Payments(IHttpClientFactory httpClientFactory, string endpoint, string endpointInternalToken, string privateApiKey, Dictionary<string, string> headers, String validateApiKey = null, String merchant = null, string request_host = null, string publicApiKey = null) : base(httpClientFactory, endpoint)
        {
            this.privateApiKey = privateApiKey;
            this.validateApiKey = validateApiKey;
            this.merchant = merchant;
            this.request_host = request_host;
            this.publicApiKey = publicApiKey;
            this.headers = headers;
            this.restClient = GetRestClient(this.headers, CONTENT_TYPE_APP_JSON);
            this.endpointInternalToken = endpointInternalToken;
        }

        #region "Instruction ThreeDS Sync and Async"    
        private PaymentResponse IntInstructionThreeDS(RestResponse result)
        {
            PaymentResponse response = null;
            Model3dsResponse model3ds = null;

            Console.WriteLine("RESULTADO DE INSTRUCTIONS: " + result.StatusCode + " " + result.Response);

            if (!String.IsNullOrEmpty(result.Response))
            {
                try
                {
                    response = JsonConvert.DeserializeObject<PaymentResponse>(result.Response);
                    if (response.status == STATUS_CHALLENGE_PENDING
                              || response.status == STATUS_FINGERPRINT_PENDING)
                    {
                        model3ds = JsonConvert.DeserializeObject<Model3dsResponse>
                        (result.Response);
                    }
                }
                catch (JsonReaderException j)
                {
                    Console.WriteLine("ERROR DE CASTEO: " + j.ToString());
                    ErrorResponse ErrorPaymentResponse = new ErrorResponse();
                    ErrorPaymentResponse.code = "502";
                    ErrorPaymentResponse.error_type = "Error en recepción de mensaje";
                    ErrorPaymentResponse.message = "No se pudo leer la respuesta";
                    ErrorPaymentResponse.validation_errors = null;
                    throw new PaymentResponseException(ErrorPaymentResponse.code, ErrorPaymentResponse);
                }
            }

            if (response != null)
            {
                response.statusCode = result.StatusCode;
            }
            if (result.StatusCode == STATUS_ACCEPTED)
            {
                throw new PaymentAuth3dsResponseException(result.StatusCode + " - " + result.Response, model3ds, result.StatusCode);
            }
            else if (result.StatusCode != STATUS_CREATED)
            {
                if (isErrorResponse(result.StatusCode))
                    throw new PaymentResponseException(result.StatusCode.ToString(), JsonConvert.DeserializeObject<ErrorResponse>(result.Response), result.StatusCode);
                else
                    throw new PaymentResponseException(result.StatusCode + " - " + result.Response, response, result.StatusCode);
            }

            return response;
        }
        public PaymentResponse InstructionThreeDS(string xConsumerUsername, Instruction3dsData instruction3DsData)
        {
            this.headers.Add("X-Consumer-Username", xConsumerUsername);
            this.restClient = GetRestClient(headers, CONTENT_TYPE_APP_JSON);
            return IntInstructionThreeDS(this.restClient.Post("threeds/instruction", toJson(instruction3DsData)));

        }
        public async Task<PaymentResponse> InstructionThreeDSAsync(string xConsumerUsername, Instruction3dsData instruction3DsData, CancellationToken cancellationToken)
        {
            this.headers.Add("X-Consumer-Username", xConsumerUsername);
            this.restClient = GetRestClient(headers, CONTENT_TYPE_APP_JSON);
            return IntInstructionThreeDS(await this.restClient.PostAsync("threeds/instruction", toJson(instruction3DsData), cancellationToken));

        }
        #endregion
        #region "Capture Paymnent Sync and Async"   
        private CapturePaymentResponse IntCapturePayment(RestResponse result)
        {
            CapturePaymentResponse response = null;
            if (result.StatusCode != STATUS_NOCONTENT && result.StatusCode != STATUS_OK)
            {
                if (isErrorResponse(result.StatusCode))
                    throw new ResponseException(result.StatusCode.ToString(), JsonConvert.DeserializeObject<ErrorResponse>(result.Response));
                else
                    throw new ResponseException(result.StatusCode + " - " + result.Response);
            }
            else
            {
                if (!String.IsNullOrEmpty(result.Response))
                {
                    response = JsonConvert.DeserializeObject<CapturePaymentResponse>(result.Response);
                }
            }
            return response;
        }
        public CapturePaymentResponse CapturePayment(long paymentId, long amount)
        {
            return IntCapturePayment(this.restClient.Put(String.Format("payments/{0}", paymentId.ToString()), "{\"amount\": " + amount.ToString() + " }"));
        }
        public async Task<CapturePaymentResponse> CapturePaymentAsync(long paymentId, long amount, CancellationToken cancellationToken)
        {
            return IntCapturePayment(await this.restClient.PutAsync(String.Format("payments/{0}", paymentId.ToString()), "{\"amount\": " + amount.ToString() + " }", cancellationToken));
        }
        #endregion
        #region "GetAllPayments Sync and Async" 
        private GetAllPaymentsResponse IntGetAllPayments(RestResponse result)
        {
            GetAllPaymentsResponse payments = null;
            if (result.StatusCode == STATUS_OK && !String.IsNullOrEmpty(result.Response))
            {
                payments = JsonConvert.DeserializeObject<GetAllPaymentsResponse>(result.Response);
            }
            else
            {
                if (isErrorResponse(result.StatusCode))
                    throw new ResponseException(result.StatusCode.ToString(), JsonConvert.DeserializeObject<ErrorResponse>(result.Response));
                else
                    throw new ResponseException(result.StatusCode + " - " + result.Response);
            }

            return payments;
        }
        public GetAllPaymentsResponse GetAllPayments(long? offset = null, long? pageSize = null, string siteOperationId = null, string merchantId = null)
        {
            GetAllPaymentsResponse payments = null;
            string queryString = GetAllPaymentsQuery(offset, pageSize, siteOperationId, merchantId);
            return IntGetAllPayments(this.restClient.Get("payments", queryString));
        }
        public async Task<GetAllPaymentsResponse> GetAllPaymentsAsync(long? offset = null, long? pageSize = null, string siteOperationId = null, string merchantId = null, CancellationToken cancellationToken = default)
        {
            GetAllPaymentsResponse payments = null;
            string queryString = GetAllPaymentsQuery(offset, pageSize, siteOperationId, merchantId);
            return IntGetAllPayments(await this.restClient.GetAsync("payments", queryString, cancellationToken));
        }
        #endregion
        #region "GetPaymentInfo Sync and Async" 
        private PaymentResponse IntGetPaymentInfo(RestResponse result)
        {
            PaymentResponse payment = null;
            if (result.StatusCode == STATUS_OK && !String.IsNullOrEmpty(result.Response))
            {
                payment = JsonConvert.DeserializeObject<PaymentResponseExtend>(result.Response);
            }
            else
            {
                if (isErrorResponse(result.StatusCode))
                    throw new ResponseException(result.StatusCode.ToString(), JsonConvert.DeserializeObject<ErrorResponse>(result.Response));
                else
                    throw new ResponseException(result.StatusCode + " - " + result.Response);
            }

            return payment;
        }

        public PaymentResponse GetPaymentInfo(long paymentId)
        {
            return IntGetPaymentInfo(this.restClient.Get("payments", String.Format("/{0}?expand=card_data", paymentId.ToString())));
        }

        public async Task<PaymentResponse> GetPaymentInfoAsync(long paymentId, CancellationToken cancellationToken)
        {
            return IntGetPaymentInfo(await this.restClient.GetAsync("payments", String.Format("/{0}?expand=card_data", paymentId.ToString()), cancellationToken));
        }
        #endregion 
        #region "Refund, DeleteRefund And DeletePartial Sync and Async"
        #region "Refund"
        private RefundPaymentResponse IntRefund(RestResponse result)
        {
            RefundPaymentResponse refund = null;
            if (result.StatusCode == STATUS_CREATED && !String.IsNullOrEmpty(result.Response))
            {
                refund = JsonConvert.DeserializeObject<RefundPaymentResponse>(result.Response);
            }
            else
            {
                throw new ResponseException(result.StatusCode.ToString(), JsonConvert.DeserializeObject<ErrorResponse>(result.Response));
            }
            return refund;
        }
        public RefundPaymentResponse Refund(long paymentId, string refundBody)
        {
            return IntRefund(this.restClient.Post(String.Format("payments/{0}/refunds", paymentId.ToString()), refundBody));
        }
        public async Task<RefundPaymentResponse> RefundAsync(long paymentId, string refundBody, CancellationToken cancellationToken)
        {
            return IntRefund(await this.restClient.PostAsync(String.Format("payments/{0}/refunds", paymentId.ToString()), refundBody, cancellationToken));
        }
        #endregion

        #region "DeleteRefund"
        public RefundResponse IntDeleteRefund(RestResponse result)
        {
            RefundResponse refund = null;
            if (result.StatusCode == STATUS_OK && !String.IsNullOrEmpty(result.Response))
            {
                refund = JsonConvert.DeserializeObject<RefundResponse>(result.Response);
            }
            else
            {
                if (isErrorResponse(result.StatusCode))
                    throw new ResponseException(result.StatusCode.ToString(), JsonConvert.DeserializeObject<ErrorResponse>(result.Response));
                else
                    throw new ResponseException(result.StatusCode + " - " + result.Response);
            }
            return refund;
        }
        public RefundResponse DeleteRefund(long paymentId, long? refundId)
        {
            return IntRefund(this.restClient.Delete(String.Format("payments/{0}/refunds/{1}", paymentId.ToString(), refundId.ToString())));
        }
        public async Task<RefundResponse> DeleteRefundAsync(long paymentId, long? refundId, CancellationToken cancellationToken)
        {
            return IntRefund(await this.restClient.DeleteAsync(String.Format("payments/{0}/refunds/{1}", paymentId.ToString(), refundId.ToString()), cancellationToken));
        }
        #endregion

        #region "DeletePartialRefund"
        public RefundResponse DeletePartialRefund(long paymentId, long? refundId)
        {
            return DeleteRefund(paymentId, refundId);
        }
        public async Task<RefundResponse> DeletePartialRefundAsync(long paymentId, long? refundId, CancellationToken cancellationToken)
        {
            return await DeleteRefundAsync(paymentId, refundId, cancellationToken);
        }

        #endregion
        #endregion
        #region "DoPayment Sync and Async"  

        public PaymentResponse ExecutePayment(OfflinePayment payment)
        {
            Payment paymentCopy = payment.copyOffline();

            return DoPayment(paymentCopy);
        }

        public PaymentResponse ExecutePayment(Payment payment)
        {
            return DoPayment(payment);

        }
        public async Task<PaymentResponse> ExecutePaymentAsync(OfflinePayment payment, CancellationToken cancellationToken)
        {
            Payment paymentCopy = payment.copyOffline();
            return await DoPaymentAsync(paymentCopy, cancellationToken);
        }

        public async Task<PaymentResponse> ExecutePaymentAsync(Payment payment, CancellationToken cancellationToken)
        {
            return await DoPaymentAsync(payment, cancellationToken);
        }

        private PaymentResponse IntDoPayment(Payment paymentCopy, RestResponse result)
        {
            PaymentResponse response = null;
            Model3dsResponse model3ds = null;


            Console.WriteLine("RESULTADO DE PAYMENT: " + result.StatusCode + " " + result.Response);

            if (!String.IsNullOrEmpty(result.Response))
            {
                try
                {
                    if (paymentCopy.cardholder_auth_required)
                    {

                        response = JsonConvert.DeserializeObject<PaymentResponse>
                            (result.Response);

                        if (response.status == STATUS_CHALLENGE_PENDING
                               || response.status == STATUS_FINGERPRINT_PENDING)
                        {
                            model3ds = JsonConvert.DeserializeObject<Model3dsResponse>
                            (result.Response);
                        }
                    }
                    else
                    {
                        response = JsonConvert.DeserializeObject<PaymentResponse>(result.Response);

                    }
                }
                catch (JsonReaderException j)
                {
                    Console.WriteLine("ERROR DE CASTEO: " + j.ToString());
                    ErrorResponse ErrorPaymentResponse = new ErrorResponse();
                    ErrorPaymentResponse.code = "502";
                    ErrorPaymentResponse.error_type = "Error en recepción de mensaje";
                    ErrorPaymentResponse.message = "No se pudo leer la respuesta";
                    ErrorPaymentResponse.validation_errors = null;
                    throw new PaymentResponseException(ErrorPaymentResponse.code, ErrorPaymentResponse);
                }
            }
            if (response != null)
            {

                response.statusCode = result.StatusCode;
            }

            if (result.StatusCode != STATUS_CREATED)
            {
                if (result.StatusCode == STATUS_ERROR)
                {
                    throw new PaymentResponseException(result.StatusCode + " - " + result.Response, JsonConvert.DeserializeObject<ErrorResponse>(result.Response), result.StatusCode);
                }
                else
                {

                    if (isErrorResponse(result.StatusCode))
                    {

                        throw new PaymentResponseException(result.StatusCode.ToString(), JsonConvert.DeserializeObject<ErrorResponse>(result.Response), result.StatusCode);
                    }
                    else
                    {
                        if (paymentCopy.cardholder_auth_required)
                        {
                            if (result.StatusCode == STATUS_ACCEPTED)
                            {
                                throw new PaymentAuth3dsResponseException(result.StatusCode + " - " + result.Response, model3ds, result.StatusCode);
                            }
                            else
                            {
                                throw new PaymentResponseException(result.StatusCode + " - " + result.Response, response, result.StatusCode);
                            }
                        }
                    }

                }
            }


            return response;
        }

        private PaymentResponse DoPayment(Payment paymentCopy)
        {
            return IntDoPayment(paymentCopy, this.restClient.Post("payments", Payment.toJson(paymentCopy)));
        }

        private async Task<PaymentResponse> DoPaymentAsync(Payment paymentCopy, CancellationToken cancellationToken)
        {
            return IntDoPayment(paymentCopy, await this.restClient.PostAsync("payments", Payment.toJson(paymentCopy), cancellationToken));
        }


        #endregion
        #region "Payment Validation"
        public ValidateResponse ValidatePayment(ValidateData validateData)
        {
            return DoValidate(validateData);
        }
        public async Task<ValidateResponse> ValidatePaymentAsync(ValidateData validateData, CancellationToken cancellationToken)
        {
            return await DoValidateAsync(validateData, cancellationToken);
        }
        private ValidateResponse IntDoValidate(RestResponse result)
        {
            ValidateResponse response = null;
            if (!String.IsNullOrEmpty(result.Response))
            {
                response = JsonConvert.DeserializeObject<ValidateResponse>(result.Response);
            }

            response.statusCode = result.StatusCode;

            if (result.StatusCode != STATUS_CREATED)
            {
                if (isErrorResponse(result.StatusCode))
                    throw new ValidateResponseException(result.StatusCode.ToString(), JsonConvert.DeserializeObject<ErrorResponse>(result.Response));
                else
                    throw new ValidateResponseException(result.StatusCode + " - " + result.Response, response);
            }

            return response;
        }
        private ValidateResponse DoValidate(ValidateData validatePayment)
        {
            this.headers["apikey"] = this.validateApiKey;
            this.headers.Add("X-Consumer-Username", this.merchant);
            var restClientValidate = GetRestClient(this.request_host + "/web/", headers, CONTENT_TYPE_APP_JSON);
            return IntDoValidate(restClientValidate.Post("validate", ValidateData.toJson(validatePayment)));
        }
        private async Task<ValidateResponse> DoValidateAsync(ValidateData validatePayment, CancellationToken cancellationToken)
        {
            this.headers["apikey"] = this.validateApiKey;
            this.headers.Add("X-Consumer-Username", this.merchant);
            var restClientValidate = GetRestClient(this.request_host + "/web/", headers, CONTENT_TYPE_APP_JSON);
            return IntDoValidate(await restClientValidate.PostAsync("validate", ValidateData.toJson(validatePayment), cancellationToken));

        }
        #endregion
        #region "GetInternalToken Sync and Async"    
        public GetInternalTokenResponse GetInternalToken(InternalTokenRequest token)
        {
            return DoGetInternalToken(InternalTokenRequest.toJson(token));
        }
        public async Task<GetInternalTokenResponse> GetInternalTokenAsync(InternalTokenRequest token, CancellationToken cancellationToken)
        {
            return await DoGetInternalTokenAsync(InternalTokenRequest.toJson(token), cancellationToken);
        }
        private static GetInternalTokenResponse IntDoGetInternalToken(RestResponse result)
        {
            GetInternalTokenResponse response = null;
            if (result.StatusCode == STATUS_CREATED)
            {
                if (!String.IsNullOrEmpty(result.Response))
                {
                    response = JsonConvert.DeserializeObject<GetInternalTokenResponse>(result.Response);
                }

            }
            else
            {
                throw new GetTokenResponseException(result.StatusCode.ToString(), JsonConvert.DeserializeObject<ErrorInternalTokenResponse>(result.Response), result.StatusCode);
            }

            return response;

        }
        private GetInternalTokenResponse DoGetInternalToken(string cardTokenJson)
        {
            this.headers["apikey"] = this.publicApiKey;
            var restClientGetTokenBSA = GetRestClient(this.endpointInternalToken, this.headers, CONTENT_TYPE_APP_JSON);
            return IntDoGetInternalToken(restClientGetTokenBSA.Post("tokens", cardTokenJson));
        }
        private async Task<GetInternalTokenResponse> DoGetInternalTokenAsync(string cardTokenJson, CancellationToken cancellationToken)
        {
            this.headers["apikey"] = this.publicApiKey;
            var restClientGetTokenBSA = GetRestClient(this.endpointInternalToken, this.headers, CONTENT_TYPE_APP_JSON);
            return IntDoGetInternalToken(await restClientGetTokenBSA.PostAsync("tokens", cardTokenJson, cancellationToken));
        }
        #endregion
        #region "Cryptogram Sync and Async"
        public GetCryptogramResponse GetCryptogram(CryptogramRequest cryptogramRequest)
        {
            return DoGetCryptogram(toJson(cryptogramRequest));
        }
        public async Task<GetCryptogramResponse> GetCryptogramAsync(CryptogramRequest cryptogramRequest, CancellationToken cancellationToken)
        {
            return await DoGetCryptogramAsync(toJson(cryptogramRequest), cancellationToken);
        }

        private static GetCryptogramResponse IntDoGetCryptogram(RestResponse result)
        {
            GetCryptogramResponse response = null;
            if (result.StatusCode == STATUS_CREATED)
            {
                if (!String.IsNullOrEmpty(result.Response))
                {
                    response = JsonConvert.DeserializeObject<GetCryptogramResponse>(result.Response);
                }

            }
            else
            {
                throw new GetTokenResponseException(result.StatusCode.ToString(), JsonConvert.DeserializeObject<ErrorInternalTokenResponse>(result.Response), result.StatusCode);
            }

            return response;

        }
        private GetCryptogramResponse DoGetCryptogram(string cryptogramJson)
        {
            this.headers["apikey"] = this.privateApiKey;
            var restClientGetCryptogram = GetRestClient(this.endpointInternalToken, this.headers, CONTENT_TYPE_APP_JSON);
            return IntDoGetCryptogram(restClientGetCryptogram.Post("payments", cryptogramJson));
        }
        private async Task<GetCryptogramResponse> DoGetCryptogramAsync(string cryptogramJson, CancellationToken cancellationToken)
        {
            this.headers["apikey"] = this.privateApiKey;
            var restClientGetCryptogram = GetRestClient(this.endpointInternalToken, this.headers, CONTENT_TYPE_APP_JSON);
            return IntDoGetCryptogram(await restClientGetCryptogram.PostAsync("payments", cryptogramJson, cancellationToken));
        }

        #endregion
        #region "GetToken Sync and Async"   
        public GetTokenResponse GetToken(TokenRequest token)
        {
            string cardTokenJson = TokenRequest.toJson(token);
            return DoGetToken(cardTokenJson);

        }
        public async Task<GetTokenResponse> GetTokenAsync(TokenRequest token, CancellationToken cancellationToken)
        {
            string cardTokenJson = TokenRequest.toJson(token);
            return await DoGetTokenAsync(cardTokenJson, cancellationToken);

        }
        private GetTokenResponse IntDoGetToken(RestResponse result)
        {
            GetTokenResponse response = null;
            if (!String.IsNullOrEmpty(result.Response))
            {
                response = JsonConvert.DeserializeObject<GetTokenResponse>(result.Response);
            }

            if (result.StatusCode != STATUS_CREATED)
            {
                if (isErrorResponse(result.StatusCode))
                    throw new GetTokenResponseException(result.StatusCode.ToString(), JsonConvert.DeserializeObject<ErrorResponse>(result.Response));
                else
                    throw new GetTokenResponseException(result.StatusCode + " - " + result.Response, response);
            }

            return response;

        }
        private GetTokenResponse DoGetToken(string cardTokenJson)
        {
            this.headers["apikey"] = this.publicApiKey;
            return IntDoGetToken(GetRestClient(this.headers, CONTENT_TYPE_APP_JSON).Post("tokens", cardTokenJson));
        }
        private async Task<GetTokenResponse> DoGetTokenAsync(string cardTokenJson, CancellationToken cancellationToken)
        {
            this.headers["apikey"] = this.publicApiKey;
            return IntDoGetToken(await GetRestClient(this.headers, CONTENT_TYPE_APP_JSON).PostAsync("tokens", cardTokenJson, cancellationToken));

        }

        public GetTokenResponse GetTokenByCardTokenBsa(CardTokenBsa card_token)
        {
            string cardTokenJson = CardTokenBsa.toJson(card_token);
            return DoGetToken(cardTokenJson);
        }
        public async Task<GetTokenResponse> GetTokenByCardTokenBsaAsync(CardTokenBsa card_token, CancellationToken cancellationToken)
        {
            string cardTokenJson = CardTokenBsa.toJson(card_token);
            return await DoGetTokenAsync(cardTokenJson, cancellationToken);
        }


        #endregion
        #region "Utilities"

        private static string toJson(Object payment)
        {
            return JsonConvert.SerializeObject(payment, Newtonsoft.Json.Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        private static string GetAllPaymentsQuery(long? offset, long? pageSize, string siteOperationId, string merchantId)
        {
            StringBuilder result = new StringBuilder();
            bool isNotNull = false;
            result.Append("?");

            if (offset != null)
            {
                isNotNull = true;
                result.Append(string.Format("{0}={1}", "offset", offset));
            }

            if (pageSize != null)
            {
                isNotNull = true;
                result.Append(string.Format("{0}={1}", "pageSize", pageSize));
            }

            if (!String.IsNullOrEmpty(siteOperationId))
            {
                isNotNull = true;
                result.Append(string.Format("{0}={1}", "siteOperationId", siteOperationId));
            }

            if (!String.IsNullOrEmpty(merchantId))
            {
                isNotNull = true;
                result.Append(string.Format("{0}={1}", "merchantId", merchantId));
            }

            if (isNotNull)
                return result.ToString();

            return String.Empty;
        }


        #endregion
    }
}
