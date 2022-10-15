using Microsoft.Extensions.Logging;

namespace Proxy.Core.Services;

public class TestDataProvider
{
  private readonly IJsonService _jsonService;
  private readonly ILogger<TestDataProvider> _logger;

  public TestDataProvider(IJsonService jsonService,
    ILogger<TestDataProvider> logger)
  {
    _jsonService = jsonService;
    _logger = logger;
  }

  public T GetTestData<T>(string path)
  {
    var testDataFilePath = $"testdata/{path}.json";
    if (!File.Exists(testDataFilePath))
    {
      _logger.LogDebug($"Failed to find debug test data: {testDataFilePath}");

      throw new FileNotFoundException($"Failed to find debug test data: {testDataFilePath}");
    }

    var text = File.ReadAllText(testDataFilePath);
    return _jsonService.Deserialize<T>(text);
  }
}
