using Proxy.Core.Model;

namespace Proxy.Core.Models;

public class VariablesModel : IVariablesModel
{
  public bool DebugEnabled { get; init; }

  public VariablesModel()
  { }
}
