using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Proxy.Core.DataModels.Dynamo;
using Proxy.Core.Services;

namespace Proxy.Command;

internal class SubRepository
{
  private readonly IDynamoService _dbService;
  private readonly ILogger<SubRepository> _logger;
  private readonly IJsonService _jsonService;

  public SubRepository(IDynamoService dbService,
    ILogger<SubRepository> logger,
    IJsonService jsonService)
  {
    _dbService = dbService;
    _jsonService = jsonService;
    _logger = logger;
  }

  public async Task<bool> Add(string userId, string channelId, string espAreaId)
  {
    _logger.LogDebug($"Adding subscription for {userId}:{channelId}:{espAreaId}");

    var (userExists, userRecord) = await _dbService.GetItem(TableNames.Sub, userId);

    // var rawNewRecord = new AttributeValue
    // {
    //   M = new Dictionary<string, AttributeValue>
    //   {
    //     {
    //       guildId,
    //       new AttributeValue
    //       {
    //         M = new Dictionary<string, AttributeValue>
    //         {
    //           { channelId, new AttributeValue { SS = new List<string> { espAreaId } } }
    //         }
    //       }
    //     }
    //   }
    // };
    var rawNewRecord = new AttributeValue
    {
      M = new Dictionary<string, AttributeValue>
      {
        { channelId, new AttributeValue { SS = new List<string> { espAreaId } } }
      }
    };

    var newItem = new Dictionary<string, AttributeValue>
    {
      { TableNameColumns.UserId, new AttributeValue(userId) }
    };

    if (userExists)
    {
      var userSubMappings = userRecord[TableNameColumns.SubMappings];
      
      var changes = Combine(ref userSubMappings, rawNewRecord, channelId, espAreaId);
      if (!changes) return false;

      newItem.Add(TableNameColumns.SubMappings, userSubMappings);
    }
    else
    {
      newItem.Add(TableNameColumns.SubMappings, rawNewRecord);
    }

    try
    {
      var request = new PutItemRequest
      {
        TableName = TableNames.Sub,
        ReturnValues = ReturnValue.NONE,
        Item = newItem
      };

      var success = await _dbService.PutItem(request);

      return success;
    }
    catch (Exception)
    {
      // ignore
    }

    return false;
  }

  private bool Combine(ref AttributeValue source, AttributeValue dest, string channelId, string areaId)
  {
    // Channel
    if (!source.M.ContainsKey(channelId))
    {
      source.M.Add(channelId, dest.M[channelId]);
      return true;
    }

    // Area id in list
    if (!source.M[channelId].SS.Contains(areaId))
    {
      source.M[channelId].SS.Add(areaId);
      return true;
    }

    return false;
  }

  // public async Task<List<string>> List(UserSeriesListRequest request)
  // {
  //   try
  //   {
  //     var (getSuccess, item) = await _dbService.GetItem(TableNames.Users, request.UserSub);
  //
  //     if (getSuccess)
  //     {
  //       return item["TvdbSub"].SS;
  //     }
  //   }
  //   catch (Exception ex)
  //   {
  //   }
  //
  //   return default;
  // }
  //
  // public async Task<bool> Delete(UserSeriesDeleteRequest request)
  // {
  //   try
  //   {
  //     var updateRequest = new UpdateItemRequest
  //     {
  //       TableName = TableNames.Users,
  //       Key = new Dictionary<string, AttributeValue>
  //       {
  //         { TableNameKeys.Users, new AttributeValue(request.UserSub) }
  //       },
  //       ExpressionAttributeNames = new Dictionary<string, string>
  //       {
  //         {"#TS", TableNameKeys.Series}
  //       },
  //       ExpressionAttributeValues = new Dictionary<string, AttributeValue>
  //       {
  //         {":tvdbsub", new AttributeValue { SS = { request.TvdbSub }}},
  //       },
  //       UpdateExpression = "DELETE #TS :tvdbsub"
  //     };
  //
  //     var (updateSuccess, userItem) = await _dbService.UpdateItem(updateRequest);
  //
  //     if (updateSuccess)
  //     {
  //       var d1 = _jsonService.DebugObject(userItem);
  //     }
  //
  //     return updateSuccess;
  //   }
  //   catch (Exception ex)
  //   {
  //   }
  //
  //   return false;
  // }
}
