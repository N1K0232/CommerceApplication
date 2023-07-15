using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CommerceApi.DataAccessLayer.Interceptors;

public class SqlCommandInterceptor : DbCommandInterceptor
{
    public override DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
    {
        var command = base.CommandCreated(eventData, result);
        return command;
    }
}