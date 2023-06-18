using System.Data.Common;

namespace CommerceApi.DataAccessLayer.Extensions;

public static class DbCommandExtensions
{
    public static DbCommand WithSqlParameter(this DbCommand command, string parameterName, object paramValue)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = parameterName;
        parameter.Value = paramValue;

        command.Parameters.Add(parameter);
        return command;
    }
}