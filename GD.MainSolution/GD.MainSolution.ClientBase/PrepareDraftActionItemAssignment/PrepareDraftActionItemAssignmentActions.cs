using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.PrepareDraftActionItemAssignment;

namespace GD.MainSolution.Client
{
  partial class PrepareDraftActionItemAssignmentActions
  {

    public virtual void CreateCoverLettersForTransfer(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var resolution = MainSolution.ActionItemExecutionTasks.As(_obj.Task);
      if (resolution != null)
      {
        var actionItem = MainSolution.ActionItemExecutionTasks.As(_obj.DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault());
        var coverLetter = MainSolution.Functions.ActionItemExecutionTask.CreateCoverLetterForExecution(resolution, actionItem, e);
        if (coverLetter != null && !_obj.CoverDocumentsGroup.OfficialDocuments.Contains(coverLetter))
        {
          _obj.CoverDocumentsGroup.OfficialDocuments.Add(coverLetter);
          _obj.Save();
        }
        
        var notificationTransfer = MainSolution.Functions.ActionItemExecutionTask.CreateTransferNotificationForExecution(resolution, actionItem, e);
        if (notificationTransfer != null && !_obj.CoverDocumentsGroup.OfficialDocuments.Contains(notificationTransfer))
        {
          _obj.CoverDocumentsGroup.OfficialDocuments.Add(notificationTransfer);
          _obj.Save();
        }
      }
    }

    public virtual bool CanCreateCoverLettersForTransfer(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.DraftActionItemGroup.ActionItemExecutionTasks.Any() &&
        MainSolution.Requests.Is(_obj.DocumentsGroup.OfficialDocuments.FirstOrDefault());

    }

    public virtual void OpenActionItem(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var draftActionItem = _obj.DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault();
      if (draftActionItem != null)
        draftActionItem.ShowModal();
    }

    public virtual bool CanOpenActionItem(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }


    public virtual void AddActionItem(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var newTask = GD.MainSolution.PublicFunctions.PrepareDraftActionItemAssignment.Remote.CreateActionItem(_obj);
      
      if (newTask != null)
      {
        newTask.ShowModal();
        
        if (!newTask.State.IsInserted)
        {
          _obj.DraftActionItemGroup.ActionItemExecutionTasks.Add(newTask);
          _obj.Save();
        }
      }
    }

    public virtual bool CanAddActionItem(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == Sungero.Workflow.Assignment.Status.InProcess && _obj.DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault() == null;
    }

    public virtual void SendForExecute(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var draftActionItem = _obj.DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault();
      if (Sungero.RecordManagement.PublicFunctions.ActionItemExecutionTask.CheckOverdueActionItemExecutionTask(draftActionItem))
      {
        e.AddError(GD.MainSolution.ActionItemExecutionTasks.Resources.PerformerDeadlineLessThenTodayCorrectIt);
        e.Cancel();
      }
      else if((draftActionItem != null &&
               draftActionItem.CoAssignees.Any() &&
               draftActionItem.CoAssigneesDeadline <= Calendar.Now) ||
              (draftActionItem != null &&
               draftActionItem.ActionItemParts.Any(x => x.CoAssigneesDeadline <= Calendar.Now)))
      {
        e.AddError(GD.MainSolution.ActionItemExecutionTasks.Resources.CoexecutorDeadlineLessThenTodayCorrectIt);
        e.Cancel();
      }
      
      var lockInfo = Locks.GetLockInfo(draftActionItem);
      if (draftActionItem != null && lockInfo.IsLocked)
      {
        e.AddError(GD.MainSolution.PrepareDraftActionItemAssignments.Resources.ActionItemLockedFormat(lockInfo.OwnerName),
                   _obj.Info.Actions.OpenActionItem);
        e.Cancel();
      }
      
      if (CitizenRequests.Requests.Is(_obj.DocumentsGroup.OfficialDocuments.FirstOrDefault()))
      {
        var task = MainSolution.ActionItemExecutionTasks.As(_obj.Task);
        var actionItemTasks = _obj.DraftActionItemGroup.ActionItemExecutionTasks;
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
        }
      }
    }

    public virtual bool CanSendForExecute(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault() != null;
    }

    public virtual void Explored(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (!Sungero.Docflow.PublicFunctions.Module.ShowConfirmationDialog(GD.MainSolution.PrepareDraftActionItemAssignments.Resources.DoneActionItem, null, null,
                                                                         Sungero.RecordManagement.Constants.ActionItemExecutionTask.ActionItemExecutionAssignmentConfirmDialogID))
      {
        e.Cancel();
      }
      
      var executionAssignment =  ActionItemExecutionAssignments.GetAll().FirstOrDefault(j => Equals(j.Task, _obj.Task) &&
                                                                                        j.Status == Sungero.Workflow.AssignmentBase.Status.InProcess &&
                                                                                        Equals(j.Performer, ActionItemExecutionTasks.As(_obj.Task).Assignee));
      if (executionAssignment != null)
      {
        var subActionItemExecutions = Functions.ActionItemExecutionTask.Remote.GetSubActionItemExecutions(executionAssignment);
        if (subActionItemExecutions.Any())
        {
          // Диалог с выбором действий для подчиненных поручений.
          var dialog = Dialogs.CreateTaskDialog(ActionItemExecutionTasks.Resources.StopAdditionalActionItemExecutions,
                                                MessageType.Question);
          Action showNotCompletedExecutionSubTasksHandler = () =>
          {
            subActionItemExecutions.ShowModal();
          };
          
          var showNotCompletedExecutionSubTasks = dialog.AddHyperlink(ActionItemExecutionTasks.Resources.NotCompletedExecutionSubTasksHyperlinkTitle);
          showNotCompletedExecutionSubTasks.SetOnExecute(showNotCompletedExecutionSubTasksHandler);
          var notAbort = dialog.Buttons.AddCustom(ActionItemExecutionAssignments.Resources.NotAbort);
          dialog.Buttons.Default = notAbort;
          var abort = dialog.Buttons.AddCustom(ActionItemExecutionAssignments.Resources.Abort);
          dialog.Buttons.AddCancel();
          var dialogResult = dialog.Show();
          
          if (dialogResult == notAbort)
            return;
          
          if (dialogResult == abort)
          {
            _obj.NeedAbortChildActionItems = true;
            return;
          }
          
          if (dialogResult == DialogButtons.Cancel)
            e.Cancel();
        }
      }
    }

    public virtual bool CanExplored(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void SendForReview(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      var draftActionItem = _obj.DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault();
      var error = string.Empty;
      if (GD.MainSolution.ActionItemExecutionTasks.Is(draftActionItem))
        error = PublicFunctions.ActionItemExecutionTask.Remote.CheckDeadlineInResolution(GD.MainSolution.ActionItemExecutionTasks.As(draftActionItem));
      if (!string.IsNullOrEmpty(error))
      {
        e.AddError(error);
        e.Cancel();
      }
      
      if (CitizenRequests.Requests.Is(_obj.DocumentsGroup.OfficialDocuments.FirstOrDefault()))
      {
        // Выполнить проверки для перенаправления.
        var task = MainSolution.ActionItemExecutionTasks.As(_obj.Task);
        var actionItemTasks = _obj.DraftActionItemGroup.ActionItemExecutionTasks;
        var resolution = actionItemTasks.Any() ? MainSolution.ActionItemExecutionTasks.As(actionItemTasks.FirstOrDefault()) :
          MainSolution.Module.CitizenRequests.PublicFunctions.Module.Remote.GetActualActionItemExecutionTask(task);
        
        if (resolution != null && !MainSolution.Functions.ActionItemExecutionTask.CheckActualityLettersForExecution(task, resolution, e))
          e.Cancel();
      }
      
    }

    public virtual bool CanSendForReview(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault() != null;
    }

  }


}