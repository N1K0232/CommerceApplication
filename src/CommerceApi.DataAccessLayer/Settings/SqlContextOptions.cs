namespace CommerceApi.DataAccessLayer.Settings;

public class SqlContextOptions
{
    public string ConnectionString { get; set; }

    public int CommandTimeout { get; set; }
}