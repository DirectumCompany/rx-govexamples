using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ApprovalTask;

namespace GD.MainSolution.Server
{
  partial class ApprovalTaskFunctions
  {
    /// <summary>
    /// Получить поручение (связанное с вх.письмом/обращением) при создании задачи на согласование по регламенту.
    /// </summary>
    [Public, Remote]
    public IActionItemExecutionAssignment GetActionItemFromIncomingLetter()
    {
      // Для исходящх писем.
      var outLetter = Sungero.RecordManagement.OutgoingLetters.As(_obj.DocumentGroup.OfficialDocuments.FirstOrDefault());
      if (outLetter != null && outLetter.InResponseTo != null)
      {
        var incLetter = Sungero.RecordManagement.IncomingLetters.As(outLetter.InResponseTo);
        var requestInResponse = GD.CitizenRequests.Requests.As(outLetter.InResponseTo);
        if (incLetter != null)
        {
          var actionItem = incLetter.ActionItemGD;
          if (actionItem != null && actionItem.ResultGroup.OfficialDocuments.Contains(outLetter))
            return actionItem;
        }
        else if (requestInResponse != null)
        {
          var actionItem = requestInResponse.ActionItemGD;
          if (actionItem != null && actionItem.OtherGroup.All.Any(x => x.Id == outLetter.Id))
            return actionItem;          
        }
      }
      
      // Для исходящих писем по обращениям.
      var outgoingRequestLetter = GD.CitizenRequests.OutgoingRequestLetters.As(_obj.DocumentGroup.OfficialDocuments.FirstOrDefault());
      if (outgoingRequestLetter != null)
      {
        var request = GD.CitizenRequests.Requests.As(outgoingRequestLetter.Request);
        if (request != null)
        {
          var actionItem = request.ActionItemGD;
          if (actionItem != null && actionItem.ResultGroup.OfficialDocuments.Contains(outgoingRequestLetter))
            return actionItem;          
        }
      }
      
      return null;
    }
  }
}