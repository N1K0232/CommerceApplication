using System.Data;
using Microsoft.Data.Sqlite;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace CommerceApi.Logging;

public class SqliteSink : PeriodicBatchingSink
{
    private readonly string databasePath;
    private readonly string tableName;

    public SqliteSink(string databasePath, string tableName) : base(10, TimeSpan.FromSeconds(10))
    {
        this.databasePath = databasePath;
        this.tableName = tableName;

        using var connection = GetSqliteConnection();
        CreateSqlTable(connection);
    }

    protected override void EmitBatch(IEnumerable<LogEvent> events)
    {
        using var connection = GetSqliteConnection();

        var commandText = $"INSERT INTO {tableName} (Message, Level, Timestamp, Exception) " +
           $"VALUES (@message, @level, @timestamp, @exception)";

        foreach (var logEvent in events)
        {
            using var command = new SqliteCommand(commandText, connection);

            CreateParameter(command, "@message", DbType.String, logEvent.RenderMessage());
            CreateParameter(command, "@level", DbType.String, logEvent.Level.ToString());
            CreateParameter(command, "@timestamp", DbType.DateTime, logEvent.Timestamp.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            CreateParameter(command, "@exception", DbType.String, logEvent.Exception?.ToString());

            command.ExecuteNonQuery();
        }
    }

    private static void CreateParameter(IDbCommand command, string name, DbType type, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = type;
        parameter.Value = value ?? DBNull.Value;

        command.Parameters.Add(parameter);
    }

    private SqliteConnection GetSqliteConnection()
    {
        var builder = new SqliteConnectionStringBuilder { DataSource = databasePath };
        var sqliteConnection = new SqliteConnection(builder.ConnectionString);
        sqliteConnection.Open();

        return sqliteConnection;
    }

    private void CreateSqlTable(SqliteConnection sqliteConnection)
    {
        var columnDefitions = "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
           "Message TEXT, " +
           "Level VARCHAR(10), " +
           "Timestamp TEXT, " +
           "Exception TEXT";

        var sqlCreateText = $"CREATE TABLE IF NOT EXISTS {tableName} ({columnDefitions})";

        using var sqlCommand = new SqliteCommand(sqlCreateText, sqliteConnection);
        sqlCommand.ExecuteNonQuery();
    }
}