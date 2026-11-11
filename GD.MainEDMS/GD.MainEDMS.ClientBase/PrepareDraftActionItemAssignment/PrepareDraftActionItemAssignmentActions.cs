using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainEDMS.PrepareDraftActionItemAssignment;

namespace GD.MainEDMS.Client
{
  partial class PrepareDraftActionItemAssignmentActions
  {
    public virtual void CreateCoverLettersForTransfer(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var resolution = MainSolution.ActionItemExecutionTasks.As(_obj.Task.ParentTask);
      if (resolution != null)
      {
        var coverLetterAndTransferNotificationTemplates = MainSolution.Module.CitizenRequests.PublicFunctions.Module.GetCoverLetterAndTransferNotificationTemplatesByActionItem(resolution);
        if (coverLetterAndTransferNotificationTemplates == null)
          return;
        
        var actionItem = MainSolution.ActionItemExecutionTasks.As(_obj.DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault());
        var coverLetter = GD.MainSolution.PublicFunctions.ActionItemExecutionTask.CreateCoverLetterForExecution(resolution, actionItem, e, coverLetterAndTransferNotificationTemplates.CoverLetterTemplate);
        if (coverLetter != null && !_obj.CoverDocumentsGroup.OfficialDocuments.Contains(coverLetter))
        {
          _obj.CoverDocumentsGroup.OfficialDocuments.Add(coverLetter);
          _obj.Save();
        }
        
        var notificationTransfer = GD.MainSolution.PublicFunctions.ActionItemExecutionTask.CreateTransferNotificationForExecution(resolution, actionItem, e, coverLetterAndTransferNotificationTemplates.TransferNotificationTemplate);
        if (notificationTransfer != null && !_obj.CoverDocumentsGroup.OfficialDocuments.Contains(notificationTransfer))
        {
          _obj.CoverDocumentsGroup.OfficialDocuments.Add(notificationTransfer);
          _obj.Save();
        }
      }
    }

    public virtual bool CanCreateCoverLettersForTransfer(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      // Проверить, что у поручения есть перенаправление по компетенции.
      var needCoverLetters = GovernmentSolution.PublicFunctions.DocumentReviewTask.NeedCoverLetters(GovernmentSolution.ActionItemExecutionTasks.As(_obj.DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault()),
                                                                                                    _obj.MainDocumentGroup.OfficialDocuments.FirstOrDefault());
      return needCoverLetters && _obj.Status == PrepareDraftActionItemAssignment.Status.InProcess;
    }

    public virtual void AddActionItem(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var newTask = PublicFunctions.PrepareDraftActionItemAssignment.Remote.CreateActionItem(_obj);
      
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
      
      if (CitizenRequests.Requests.Is(_obj.MainDocumentGroup.OfficialDocuments.FirstOrDefault()))
      {
        var task = MainSolution.ActionItemExecutionTasks.As(_obj.Task);
        var actionItemTasks = _obj.DraftActionItemGroup.ActionItemExecutionTasks;
        var resolution = actionItemTasks.Any() ? MainSolution.ActionItemExecutionTasks.As(actionItemTasks.FirstOrDefault()) :
          MainSolution.Module.CitizenRequests.PublicFunctions.Module.Remote.GetActualActionItemExecutionTask(task);
        if (resolution != null)
        {
          var documents = GD.MainSolution.PublicFunctions.ActionItemExecutionTask.CheckActualityLettersForExecution(task, resolution, e);
          // Выполнить проверки для перенаправления.
          if (!documents.SuccessfullyChecked)
            e.Cancel();
          
          if (documents.CoverLetter != null && !_obj.CoverDocumentsGroup.OfficialDocuments.Contains(documents.CoverLetter))
            _obj.CoverDocumentsGroup.OfficialDocuments.Add(documents.CoverLetter);
          if (documents.CoverNotification != null && !_obj.CoverDocumentsGroup.OfficialDocuments.Contains(documents.CoverNotification))
            _obj.CoverDocumentsGroup.OfficialDocuments.Add(documents.CoverNotification);
          
          // Подписать документы.
          var errorText = CitizenRequests.PublicFunctions.Module.SignatureTransferDocuments(documents.CoverLetter,
                                                                                            documents.CoverNotification,
                                                                                            task.Assignee);
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
                                                                         Constants.PrepareDraftActionItemTask.ActionItemExecutionAssignmentConfirmDialogID))
      {
        e.Cancel();
      }
      
      var executionAssignment = GD.MainSolution.ActionItemExecutionAssignments.GetAll().FirstOrDefault(j => Equals(j.Task, _obj.Task.ParentTask) &&
                                                                                                       j.Status == Sungero.Workflow.AssignmentBase.Status.InProcess &&
                                                                                                       Equals(j.Performer, Sungero.RecordManagement.ActionItemExecutionTasks.As(_obj.Task.ParentTask).Assignee));
      if (executionAssignment != null)
      {
        var subActionItemExecutions = GD.MainSolution.PublicFunctions.ActionItemExecutionTask.Remote.GetSubActionItemExecutions(executionAssignment);
        if (subActionItemExecutions.Any())
        {
          // Диалог с выбором действий для подчиненных поручений.
          var dialog = Dialogs.CreateTaskDialog(GD.MainSolution.ActionItemExecutionTasks.Resources.StopAdditionalActionItemExecutions,
                                                MessageType.Question);
          Action showNotCompletedExecutionSubTasksHandler = () =>
          {
            subActionItemExecutions.ShowModal();
          };
          
          var showNotCompletedExecutionSubTasks = dialog.AddHyperlink(GD.MainSolution.ActionItemExecutionTasks.Resources.NotCompletedExecutionSubTasksHyperlinkTitle);
          showNotCompletedExecutionSubTasks.SetOnExecute(showNotCompletedExecutionSubTasksHandler);
          var notAbort = dialog.Buttons.AddCustom(GD.MainSolution.ActionItemExecutionAssignments.Resources.NotAbort);
          dialog.Buttons.Default = notAbort;
          var abort = dialog.Buttons.AddCustom(GD.MainSolution.ActionItemExecutionAssignments.Resources.Abort);
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
      
      GD.MainSolution.PublicFunctions.OfficialDocument.Remote.MarkDocumentsAsObsolete(_obj.CoverDocumentsGroup.OfficialDocuments.ToList());
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
        error = GD.MainSolution.PublicFunctions.ActionItemExecutionTask.Remote.CheckDeadlineInResolution(GD.MainSolution.ActionItemExecutionTasks.As(draftActionItem));
      if (!string.IsNullOrEmpty(error))
      {
        e.AddError(error);
        e.Cancel();
      }
      
      if (CitizenRequests.Requests.Is(_obj.MainDocumentGroup.OfficialDocuments.FirstOrDefault()))
      {
        // Выполнить проверки для перенаправления.
        var task = MainSolution.ActionItemExecutionTasks.As(_obj.Task);
        var actionItemTasks = _obj.DraftActionItemGroup.ActionItemExecutionTasks;
        var resolution = actionItemTasks.Any() ? MainSolution.ActionItemExecutionTasks.As(actionItemTasks.FirstOrDefault()) :
          GD.MainSolution.Module.CitizenRequests.PublicFunctions.Module.Remote.GetActualActionItemExecutionTask(task);
        var documents = GD.MainSolution.PublicFunctions.ActionItemExecutionTask.CheckActualityLettersForExecution(task, resolution, e);
        
        if (resolution != null && !documents.SuccessfullyChecked)
          e.Cancel();
      }
    }

    public virtual bool CanSendForReview(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return _obj.DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault() != null;
    }

  }

}