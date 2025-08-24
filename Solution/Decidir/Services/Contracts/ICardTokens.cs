using System.Threading;
using System.Threading.Tasks;

namespace Decidir.Services.Contracts
{
    internal interface ICardTokens
    {
        bool DeleteCardToken(string tokenizedCard);
        Task<bool> DeleteCardTokenAsync(string tokenizedCard, CancellationToken cancellationToken);
    }
}
