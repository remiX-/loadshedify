namespace Proxy.Core.Services;

public class TableNames
{
  public static string Sub => Environment.GetEnvironmentVariable("DB_SUB_TABLE");
}

public class TableNameKeys
{
  public static IDictionary<string, string> Keys => new Dictionary<string, string>
  {
    { TableNames.Sub, TableNameColumns.UserId }
  };
}

public struct TableNameColumns
{
  public static string UserId => "user_id";
  public static string SubMappings => "sub_mappings";
}
