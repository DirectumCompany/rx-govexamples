using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionTask;

namespace GD.MainSolution.Client
{
  partial class ActionItemExecutionTaskActions
  {
    public override void ChangeTransferActionItemGD(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      {
        if (_obj.IsTransferGD == true)
        {
          if (_obj.QuestionsForTransferGD.Count(a => a.Question != null) > 0)
          {
            var dialog = Dialogs.CreateTaskDialog(GD.GovernmentSolution.ActionItemExecutionTasks.Resources.CancelTransfer,
                                                  GD.GovernmentSolution.ActionItemExecutionTasks.Resources.TransferPointRemoved,
                                                  MessageType.Question);
            dialog.Buttons.AddYesNo();
            dialog.Buttons.Default = DialogButtons.No;
            var yesResult = dialog.Show() == DialogButtons.Yes;
            if (yesResult)
              _obj.IsTransferGD = false;
          }
          else
            _obj.IsTransferGD = false;
        }
        else
        {
          _obj.IsCompoundActionItem = true;
          _obj.IsTransferGD = true;
          
          if (_obj.ParentAssignment != null)
          {
            var resolution = MainSolution.ActionItemExecutionTasks.As(_obj.ParentAssignment.Task.ParentTask);
            if (resolution!=null)
              Functions.ActionItemExecutionTask.FillQuestionsFromResolution(_obj, resolution);
          }
          else
          {
            // Заполнить табличную часть впросами из обращения.
            Functions.ActionItemExecutionTask.FillQuestionsFromRequestTransfer(_obj);
            // Проверить вопросы на рассмотрение другим адресатом.
            Functions.ActionItemExecutionTask.CheckQuestionsforReviewOtherAddressee(_obj, null, e);
          }
        }
      }
    }

    public override bool CanChangeTransferActionItemGD(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return (_obj.State.IsInserted || Locks.GetLockInfo(_obj).IsLockedByMe) &&
        _obj.Status == Sungero.Workflow.Task.Status.Draft &&
        (_obj.IsDraftResolution == true || CitizenRequests.Requests.Is(_obj.DocumentsGroup.OfficialDocuments.FirstOrDefault()));
    }

  }

}