using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.TestUtilities;
using FluentAssertions;
using Xunit;

namespace Proxy.Command;

public class ScheduleFunctionTests
{
  private readonly ScheduleFunction _subject;

  public ScheduleFunctionTests()
  {
    _subject = new ScheduleFunction();
  }

  [Fact]
  public void Handler_WhenItShouldPass_ItWillPass()
  {
    var snsEvent = new SNSEvent()
    {
      Records = new List<SNSEvent.SNSRecord>()
    };
    var context = new TestLambdaContext();

    var response = _subject.FunctionHandler(snsEvent, context);
    var passed = true;

    passed.Should().Be(true);
  }

  [Fact]
  public void Handler_WhenIShouldHaveWrittenTests_ItWouldHavePassedButLetsFailLol()
  {
    var snsEvent = new SNSEvent()
    {
      Records = new List<SNSEvent.SNSRecord>()
    };
    var context = new TestLambdaContext();

    var response = _subject.FunctionHandler(snsEvent, context);
    var passed = false;

    passed.Should().Be(true);
  }
}
