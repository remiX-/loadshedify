using Proxy.Core.Model;

namespace Proxy.Core.Models;

public class VariablesModel : IVariablesModel
{
  public string AspNetEnvironment { get; init; }

  public bool DebugEnabled { get; init; }

  public string EspAuthToken { get; init; }

  public string S3AssetBucket { get; init; }

  public VariablesModel()
  { }
}
