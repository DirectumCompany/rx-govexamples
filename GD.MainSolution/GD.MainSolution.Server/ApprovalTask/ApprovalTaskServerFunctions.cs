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
    
    /// <summary>
    /// Проверить, есть ли в задаче на согласование по регламенту этап регистрации.
    /// </summary>
    /// <returns>true, если содержится. Иначе - false.</returns>
    public bool ContainsRegisterStage()
    {
      return GetStages(_obj).Stages.Any(s => s.StageType == Sungero.Docflow.ApprovalStage.StageType.Register);
    }
    
    /// <summary>
    /// Проверить, есть ли подходящие настройки регистрации для определения исполнителя на этапе регистрации.
    /// </summary>
    /// <returns>true, если есть. Иначе - false.</returns>
    public bool ExistsRegistrationSetting()
    {
      var document = _obj.DocumentGroup.OfficialDocuments.FirstOrDefault();
      return Sungero.Docflow.RegistrationSettings.GetAll()
        .Where(s => s.SettingType == Sungero.Docflow.RegistrationSetting.SettingType.Registration &&
               s.Status == Sungero.CoreEntities.DatabookEntry.Status.Active && Equals(s.DocumentFlow, document.DocumentKind.DocumentFlow) &&
               (!s.DocumentKinds.Any() || s.DocumentKinds.Any(k => Equals(k.DocumentKind, document.DocumentKind))) &&
               (!s.BusinessUnits.Any() || s.BusinessUnits.Any(u => Equals(u.BusinessUnit, document.BusinessUnit))) &&
               (!s.Departments.Any() || s.Departments.Any(d => Equals(d.Department, document.Department))))
        .Select(s => s.DocumentRegister.RegistrationGroup)
        .Any(s => !s.Departments.Any() || s.Departments.Any(d => Equals(d.Department, document.Department)));
    }
  }
}