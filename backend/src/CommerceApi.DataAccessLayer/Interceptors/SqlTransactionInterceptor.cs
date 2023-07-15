using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CommerceApi.DataAccessLayer.Interceptors;

public class SqlTransactionInterceptor : DbTransactionInterceptor
{
    public override ValueTask<DbTransaction> TransactionStartedAsync(DbConnection connection, TransactionEndEventData eventData, DbTransaction result, CancellationToken cancellationToken = default) => base.TransactionStartedAsync(connection, eventData, result, cancellationToken);

    public override async ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(DbConnection connection, TransactionStartingEventData eventData, InterceptionResult<DbTransaction> result, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine("starting transaction");
        return await base.TransactionStartingAsync(connection, eventData, result, cancellationToken);
    }

    public override async Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine("transaction successfully committed");
        await base.TransactionCommittedAsync(transaction, eventData, cancellationToken);
    }

    public override ValueTask<InterceptionResult> TransactionCommittingAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default) => base.TransactionCommittingAsync(transaction, eventData, result, cancellationToken);
}