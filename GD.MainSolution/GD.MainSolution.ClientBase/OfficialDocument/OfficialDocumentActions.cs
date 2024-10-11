using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.OfficialDocument;

namespace GD.MainSolution.Client
{
  partial class OfficialDocumentActions
  {
    public override void SendForApproval(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      #region Копия base.SendForApproval(e), но вместо Show() используется ShowModal()
      // Если по документу ранее были запущены задачи, то вывести соответствующий диалог.
      if (!Sungero.Docflow.PublicFunctions.OfficialDocument.NeedCreateApprovalTask(_obj))
        return;
      
      // Принудительно сохранить документ, чтобы сохранились связи. Иначе они не попадут в задачу.
      _obj.Save();
      
      var availableApprovalRules = Sungero.Docflow.PublicFunctions.OfficialDocument.Remote.GetApprovalRules(_obj).ToList();
      
      if (availableApprovalRules.Any())
      {
        var task = Sungero.Docflow.PublicFunctions.Module.Remote.CreateApprovalTask(_obj);
        task.ShowModal();
        e.CloseFormAfterAction = task != null && task.Status != Sungero.Workflow.Task.Status.Draft;
      }
      else
      {
        // Если по документу нет регламента, вывести сообщение.
        Dialogs.ShowMessage(Sungero.Docflow.OfficialDocuments.Resources.NoApprovalRuleWarning, MessageType.Warning);
        throw new OperationCanceledException();
      }
      #endregion
    }

    public override bool CanSendForApproval(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanSendForApproval(e);
    }

  }

}