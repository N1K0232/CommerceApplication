using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CommerceApi.DataAccessLayer.Interceptors;

public class SqlConnectionInterceptor : DbConnectionInterceptor
{
    public override DbConnection ConnectionCreated(ConnectionCreatedEventData eventData, DbConnection result)
    {
        var connection = base.ConnectionCreated(eventData, result);
        if (connection.State is ConnectionState.Closed)
        {
            connection.Open();
        }

        return connection;
    }

    public override async ValueTask<InterceptionResult> ConnectionDisposingAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
    {
        if (connection.State is ConnectionState.Open)
        {
            await connection.CloseAsync();
        }

        var interceptionResult = await base.ConnectionDisposingAsync(connection, eventData, result);
        return interceptionResult;
    }
}