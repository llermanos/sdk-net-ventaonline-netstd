using Decidir.Model;
using System.Threading;
using System.Threading.Tasks;

namespace Decidir.Services.Contracts
{
    internal interface IBatchClosure
    {
        BatchClosureResponse BatchClosureActive(string batchClosure);
        Task<BatchClosureResponse> BatchClosureActiveAsync(string batchClosure, CancellationToken cancellationToken);
    }
}
