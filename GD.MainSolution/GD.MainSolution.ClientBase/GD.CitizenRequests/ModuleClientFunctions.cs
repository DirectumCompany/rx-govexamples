using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace GD.MainSolution.Module.CitizenRequests.Client
{
  partial class ModuleFunctions
  {
    /// <summary>
    /// Выбрать шаблоны сопроводительного письма и уведомления о перенаправлении.
    /// </summary>
    /// <param name="actionItem">Поручение.</param>
    [Public]
    public virtual GD.CitizenRequests.Structures.Module.ICoverLetterAndTransferNotificationTemplates GetCoverLetterAndTransferNotificationTemplatesByActionItem(GD.GovernmentSolution.IActionItemExecutionTask actionItem)
    {
      var request = GD.CitizenRequests.Requests.As(actionItem.DocumentsGroup.OfficialDocuments.FirstOrDefault());
      var employee = Sungero.Company.Employees.As(actionItem?.Author);
      var requestSetting = GD.CitizenRequests.PublicFunctions.Module.Remote.GetSetting(request.BusinessUnit);
      return GetCoverLetterAndTransferNotificationTemplates(employee, requestSetting);
    }
  }
}