namespace Proxy.Core.Model;

public interface IVariablesModel
{
  bool DebugEnabled { get; }

  string AspNetEnvironment { get; }

  string EspAuthToken { get; }

  string S3AssetBucket { get;  }
}
