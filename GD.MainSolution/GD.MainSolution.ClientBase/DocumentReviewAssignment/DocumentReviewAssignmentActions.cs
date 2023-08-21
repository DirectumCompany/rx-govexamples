using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.DocumentReviewAssignment;

namespace GD.MainSolution.Client
{
  partial class DocumentReviewAssignmentActions
  {
    public override void ActionItemsSent(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var callBaseAction = true;
      
      if (MainSolution.Requests.Is(_obj.DocumentForReviewGroup.OfficialDocuments.FirstOrDefault()))
      {
        var task = MainSolution.ActionItemExecutionTasks.As(_obj.Task);
        if (task != null)
        {
          var actionItemTasks = _obj.ResolutionGroup.ActionItemExecutionTasks;
          var resolution = actionItemTasks.Any() ? MainSolution.ActionItemExecutionTasks.As(actionItemTasks.FirstOrDefault()) :
            MainSolution.Module.CitizenRequests.PublicFunctions.Module.Remote.GetActualActionItemExecutionTask(task);
          if (resolution != null)
          {
            // Выполнить проверки для перенаправления.
            if (!MainSolution.Functions.ActionItemExecutionTask.CheckActualityLettersForExecution(task, resolution, e))
              e.Cancel();
            
            // Подписать документы.
            var errorText = MainSolution.Module.CitizenRequests.PublicFunctions.Module.SignatureTransferDocumentsForExecution(task, resolution);
            if (!string.IsNullOrEmpty(errorText))
            {
              e.AddError(errorText);
              e.Cancel();
            }
            
            // Если рассматривается обращение и оно полностью перенаправлено, то не показываем диалог подтверждения выполнения без отправки поручений.
            if (resolution.IsTransferGD == true && !resolution.ActionItemParts.Any())
            {
              #region Копия base.ActionItemsSent(e), но без вызова ShowConfirmationDialogCreationActionItem
              if (!Sungero.Docflow.PublicFunctions.Module
                  .ShowDialogGrantAccessRightsWithConfirmationDialog(_obj, _obj.OtherGroup.All.ToList(),
                                                                     e.Action,
                                                                     Sungero.RecordManagement.Constants.DocumentReviewTask.ReviewManagerAssignmentConfirmDialogID.AddAssignment))
                e.Cancel();
              #endregion
              callBaseAction = false;
            }
          }
        }
      }
      
      if (callBaseAction)
        base.ActionItemsSent(e);
      
    }

    public override bool CanActionItemsSent(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return base.CanActionItemsSent(e);
    }

    public override void CreateNotificationForTransferGD(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (MainSolution.DocumentReviewTasks.Is(_obj.Task))
        base.CreateNotificationForTransferGD(e);
      else
      {
        var resolution =  MainSolution.ActionItemExecutionTasks.As(_obj.Task);
        if (resolution != null)
        {
          var actionItem = MainSolution.ActionItemExecutionTasks.As(_obj.ResolutionGroup.ActionItemExecutionTasks.FirstOrDefault());
          MainSolution.Functions.ActionItemExecutionTask.CreateTransferNotificationForExecution(resolution, actionItem, e);
        }
      }
    }

    public override bool CanCreateNotificationForTransferGD(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateNotificationForTransferGD(e);
    }

    public override void CreateCoverLetterForTransferGD(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (MainSolution.DocumentReviewTasks.Is(_obj.Task))
        base.CreateCoverLetterForTransferGD(e);
      else
      {
        var resolution =  MainSolution.ActionItemExecutionTasks.As(_obj.Task);
        if (resolution != null)
        {
          var actionItem = MainSolution.ActionItemExecutionTasks.As(_obj.ResolutionGroup.ActionItemExecutionTasks.FirstOrDefault());
          MainSolution.Functions.ActionItemExecutionTask.CreateCoverLetterForExecution(resolution, actionItem, e);
        }
      }
    }

    public override bool CanCreateCoverLetterForTransferGD(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateCoverLetterForTransferGD(e);
    }

    public virtual void OpenActionItemGD(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.ResolutionGroup.ActionItemExecutionTasks.FirstOrDefault() != null)
        _obj.ResolutionGroup.ActionItemExecutionTasks.FirstOrDefault().ShowModal();
    }

    public virtual bool CanOpenActionItemGD(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public override void DraftResApprove(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (ActionItemExecutionTasks.Is(_obj.Task))
      {
        var draftActionItem = _obj.ResolutionGroup.ActionItemExecutionTasks.FirstOrDefault();
        var error = string.Empty;
        if (GD.MainSolution.ActionItemExecutionTasks.Is(draftActionItem))
          error = PublicFunctions.ActionItemExecutionTask.Remote.CheckDeadlineInResolution(GD.MainSolution.ActionItemExecutionTasks.As(draftActionItem));
        if (!string.IsNullOrEmpty(error))
        {
          e.AddError(error);
          e.Cancel();
        }
        
        var lockInfo = Locks.GetLockInfo(draftActionItem);
        if (draftActionItem != null && lockInfo != null && lockInfo.IsLocked)
        {
          e.AddError(GD.MainSolution.DocumentReviewAssignments.Resources.ActionItemLockedFormat(lockInfo.OwnerName),
                     _obj.Info.Actions.OpenActionItemGD);
          e.Cancel();
        }
        
        if (MainSolution.Requests.Is(_obj.DocumentForReviewGroup.OfficialDocuments.FirstOrDefault()))
        {
          var task = MainSolution.ActionItemExecutionTasks.As(_obj.Task);
          var actionItem = GD.MainSolution.ActionItemExecutionTasks.As(draftActionItem);
          if (actionItem != null)
          {
            // Выполнить проверки для перенаправления.
            if (!MainSolution.Functions.ActionItemExecutionTask.CheckActualityLettersForExecution(task, actionItem, e))
              e.Cancel();
            
            // Подписать документы.
            var errorText = MainSolution.Module.CitizenRequests.PublicFunctions.Module.SignatureTransferDocumentsForExecution(task, actionItem);
            if (!string.IsNullOrEmpty(errorText))
            {
              e.AddError(errorText);
              e.Cancel();
            }
          }
        }
      }
      else
        base.DraftResApprove(e);
    }

    public override bool CanDraftResApprove(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return base.CanDraftResApprove(e);
    }

    public override void DraftResRework(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (ActionItemExecutionTasks.Is(_obj.Task))
      {
        // Проверить наличие замечаний в тексте.
        if (string.IsNullOrWhiteSpace(_obj.ActiveText))
        {
          e.AddError(GD.MainSolution.DocumentReviewAssignments.Resources.NeedTextToRework);
          return;
        }
      }
      else
        base.DraftResRework(e);
    }

    public override bool CanDraftResRework(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return base.CanDraftResRework(e);
    }

    public override void Forward(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      base.Forward(e);
    }

    public override bool CanForward(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      var isActionItemExecutionTask = ActionItemExecutionTasks.Is(_obj.Task);
      return !isActionItemExecutionTask && base.CanForward(e);
    }

  }




}