using Decidir.Clients;
using Decidir.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Decidir.Services.Contracts
{
    internal interface IPayments
    {
        CapturePaymentResponse CapturePayment(long paymentId, long amount);
        Task<CapturePaymentResponse> CapturePaymentAsync(long paymentId, long amount, CancellationToken cancellationToken);
        RefundResponse DeletePartialRefund(long paymentId, long? refundId);
        Task<RefundResponse> DeletePartialRefundAsync(long paymentId, long? refundId, CancellationToken cancellationToken);
        RefundResponse DeleteRefund(long paymentId, long? refundId);
        Task<RefundResponse> DeleteRefundAsync(long paymentId, long? refundId, CancellationToken cancellationToken);
        PaymentResponse ExecutePayment(OfflinePayment payment);
        PaymentResponse ExecutePayment(Payment payment);
        Task<PaymentResponse> ExecutePaymentAsync(OfflinePayment payment, CancellationToken cancellationToken);
        Task<PaymentResponse> ExecutePaymentAsync(Payment payment, CancellationToken cancellationToken);
        GetAllPaymentsResponse GetAllPayments(long? offset = null, long? pageSize = null, string siteOperationId = null, string merchantId = null);
        Task<GetAllPaymentsResponse> GetAllPaymentsAsync(long? offset = null, long? pageSize = null, string siteOperationId = null, string merchantId = null, CancellationToken cancellationToken = default);
        GetCryptogramResponse GetCryptogram(CryptogramRequest cryptogramRequest);
        Task<GetCryptogramResponse> GetCryptogramAsync(CryptogramRequest cryptogramRequest, CancellationToken cancellationToken);
        GetInternalTokenResponse GetInternalToken(InternalTokenRequest token);
        Task<GetInternalTokenResponse> GetInternalTokenAsync(InternalTokenRequest token, CancellationToken cancellationToken);
        PaymentResponse GetPaymentInfo(long paymentId);
        Task<PaymentResponse> GetPaymentInfoAsync(long paymentId, CancellationToken cancellationToken);
        GetTokenResponse GetToken(TokenRequest token);
        Task<GetTokenResponse> GetTokenAsync(TokenRequest token, CancellationToken cancellationToken);
        GetTokenResponse GetTokenByCardTokenBsa(CardTokenBsa card_token);
        Task<GetTokenResponse> GetTokenByCardTokenBsaAsync(CardTokenBsa card_token, CancellationToken cancellationToken);
        PaymentResponse InstructionThreeDS(string xConsumerUsername, Instruction3dsData instruction3DsData);
        Task<PaymentResponse> InstructionThreeDSAsync(string xConsumerUsername, Instruction3dsData instruction3DsData, CancellationToken cancellationToken);
        RefundResponse IntDeleteRefund(RestResponse result);
        RefundPaymentResponse Refund(long paymentId, string refundBody);
        Task<RefundPaymentResponse> RefundAsync(long paymentId, string refundBody, CancellationToken cancellationToken);
        ValidateResponse ValidatePayment(ValidateData validateData);
        Task<ValidateResponse> ValidatePaymentAsync(ValidateData validateData, CancellationToken cancellationToken);
    }
}
