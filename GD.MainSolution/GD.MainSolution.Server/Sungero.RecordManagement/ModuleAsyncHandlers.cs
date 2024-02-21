using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace GD.MainSolution.Module.RecordManagement.Server
{
  partial class ModuleAsyncHandlers
  {

    public virtual void UpdateDocumentDataInActionItemAssignmentGD(Module.RecordManagement.Server.AsyncHandlerInvokeArgs.UpdateDocumentDataInActionItemAssignmentGDInvokeArgs args)
    {
      Logger.DebugFormat("UpdateDocumentDataInActionItemAssignments({0}). Start.", args.DocumentId);

      var document = OfficialDocuments.GetAll(d => d.Id == args.DocumentId).FirstOrDefault();

      if (document == null)
      {
        Logger.DebugFormat("UpdateDocumentDataInActionItemAssignments({0}). Document not found", args.DocumentId);
        return;
      }

      var needRetry = false;
      var taskDocumentGroupId = Sungero.Docflow.PublicConstants.Module.TaskMainGroup.ActionItemExecutionTask;
      var tasksIds = ActionItemExecutionTasks.GetAll(t => t.AttachmentDetails
                                                     .Any(ad => ad.GroupId == taskDocumentGroupId && ad.EntityId == document.Id))
        .Select(t => t.Id)
        .ToList();

      var assignments = ActionItemExecutionAssignments.GetAll(a => tasksIds.Contains(a.Task.Id));

      foreach (var assignment in assignments)
      {
        var newSubject = Sungero.RecordManagement.Shared.ActionItemExecutionTaskFunctions
          .GetActionItemExecutionSubject(ActionItemExecutionTasks.As(assignment.Task), ActionItemExecutionTasks.Resources.ActionItemExecutionSubject);
        
        var needUpdate = newSubject != assignment.Subject;
        if (!needUpdate)
          continue;

        if (!Locks.TryLock(assignment))
        {
          needRetry = true;
          Logger.DebugFormat("UpdateDocumentDataInActionItemAssignments({0}). Assignment({1}) is locked", args.DocumentId, assignment.Id);
          continue;
        }

        try
        {
          assignment.Subject = newSubject;
          assignment.Save();
          Logger.DebugFormat("UpdateDocumentDataInActionItemAssignments({0}). Assignment({1}) updated", args.DocumentId, assignment.Id);
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("UpdateDocumentDataInActionItemAssignments({0}). An error occured while updating Assignment({1})", ex, args.DocumentId, assignment.Id);
          needRetry = true;
        }
        finally
        {
          Locks.Unlock(assignment);
        }
      }
      
      args.Retry = needRetry;
      Logger.DebugFormat("UpdateDocumentDataInActionItemAssignments({0}). Finish. NeedRetry - {1}", args.DocumentId, args.Retry);
    }

    public virtual void UpdateDocumentDataInActionItemGD(Module.RecordManagement.Server.AsyncHandlerInvokeArgs.UpdateDocumentDataInActionItemGDInvokeArgs args)
    {
      Logger.DebugFormat("UpdateDocumentDataInActionItem({0}). Start.", args.DocumentId);

      var document = OfficialDocuments.GetAll(d => d.Id == args.DocumentId).FirstOrDefault();

      if (document == null)
      {
        Logger.DebugFormat("UpdateDocumentDataInActionItem({0}). Document not found", args.DocumentId);
        return;
      }

      var actionItems = ActionItemExecutionTasks.GetAll(ai => Equals(ai.DocumentGD, document)).ToList();
      Logger.DebugFormat("UpdateDocumentDataInActionItem({0}). ActionItems to update - {1}", args.DocumentId, actionItems.Count);

      var needRetry = false;
      foreach (var actionItem in actionItems)
      {
        var needUpdate = actionItem.RegDateGD != document.RegistrationDate ||
          actionItem.RegNumberGD != document.RegistrationNumber;

        if (!needUpdate)
          continue;

        if (!Locks.TryLock(actionItem))
        {
          needRetry = true;
          Logger.DebugFormat("UpdateDocumentDataInActionItem({0}). ActionItem({1}) is locked", args.DocumentId, actionItem.Id);
          continue;
        }

        try
        {
          actionItem.RegDateGD = document.RegistrationDate;
          actionItem.RegNumberGD = document.RegistrationNumber;
          actionItem.Save();
          
          Logger.DebugFormat("UpdateDocumentDataInActionItem({0}). ActionItem({1}) updated", args.DocumentId, actionItem.Id);
        }
        catch (Exception ex)
        {
          Logger.ErrorFormat("UpdateDocumentDataInActionItem({0}). An error occured while updating ActionItem({1})", ex, args.DocumentId, actionItem.Id);
          needRetry = true;
        }
        finally
        {
          Locks.Unlock(actionItem);
        }
        
        var updateInAssignmentsAsyncHandler = AsyncHandlers.UpdateDocumentDataInActionItemAssignmentGD.Create();
        updateInAssignmentsAsyncHandler.DocumentId = document.Id;
        updateInAssignmentsAsyncHandler.ExecuteAsync();

        args.Retry = needRetry;
        Logger.DebugFormat("UpdateDocumentDataInActionItem({0}). Finish. NeedRetry - {1}", args.DocumentId, args.Retry);
      }

    }
  }
}