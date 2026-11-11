using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using GD.MainEDMS.PrepareDraftActionItemTask;

namespace GD.MainEDMS.Server.PrepareDraftActionItemTaskBlocks
{
  partial class DeleteDraftResolutionsBlockHandlers
  {

    public virtual void DeleteDraftResolutionsBlockExecute()
    {
      Logger.DebugFormat("ActionItemExecutionTask({0}) DeleteDraftResolutionsBlockExecute", _obj.Id);
      var draftActionItem = _obj.DraftActionItem;
      var actionItems = new List<MainSolution.IActionItemExecutionTask>();
      if (draftActionItem != null && draftActionItem.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.Draft &&
          !_block.ActualFromAddresseesGD.Contains(draftActionItem.AssignedBy))
      {
        actionItems.Add(MainSolution.ActionItemExecutionTasks.As(draftActionItem));
        _obj.DraftActionItem = null;
        MainSolution.Module.RecordManagement.PublicFunctions.Module.DeleteActionItemExecutionTasks(actionItems);
      }
    }
  }

  partial class PrepareDraftActionItemAssignmentBlockHandlers
  {
    public virtual void PrepareDraftActionItemAssignmentBlockStartAssignment(GD.MainEDMS.IPrepareDraftActionItemAssignment assignment)
    {
      var executionAssignment = GD.MainSolution.ActionItemExecutionAssignments.GetAll().FirstOrDefault(j => Equals(j.Task, _obj.ParentTask) &&
                                                                                                       j.Status == Sungero.Workflow.AssignmentBase.Status.InProcess &&
                                                                                                       Equals(j.Performer, Sungero.RecordManagement.ActionItemExecutionTasks.As(_obj.ParentTask).Assignee));
      if (executionAssignment != null)
        executionAssignment.AssignmentStatusGD = GD.MainSolution.ActionItemExecutionAssignment.AssignmentStatusGD.ReviewDraftGD;
      
      if (_obj.DraftActionItem != null)
      {
        assignment.DraftActionItemGroup.ActionItemExecutionTasks.Clear();
        assignment.DraftActionItemGroup.ActionItemExecutionTasks.Add(_obj.DraftActionItem);
      }
      
      var document = _obj.MainDocumentGroup.OfficialDocuments.FirstOrDefault();
      var performer = assignment.Performer;
      
      if (document != null)
      {
        Sungero.Docflow.PublicFunctions.OfficialDocument.GrantAccessRightsToActionItemAttachment(document, Sungero.Company.Employees.As(performer));
        Sungero.Docflow.PublicFunctions.Module.SynchronizeAddendaAndAttachmentsGroup(_obj.AddendumGroup, document);
      }
      
      // Выдать права на основную задачу на рассмотрение.
      if (Sungero.RecordManagement.DocumentReviewTasks.Is(_obj.MainTask) && !_obj.MainTask.AccessRights.CanUpdate(performer))
      {
        _obj.MainTask.AccessRights.Grant(performer, DefaultAccessRightsTypes.Change);
        
        if (!_obj.MainTask.AccessRights.CanUpdate(performer))
          _obj.MainTask.AccessRights.Grant(performer, DefaultAccessRightsTypes.Change);
        
        _obj.MainTask.AccessRights.Save();
      }
      else
      {
        Sungero.Workflow.ITask actionItemTask = _obj.ParentTask;
        
        while (actionItemTask.ParentTask != null || actionItemTask.ParentAssignment != null)
        {
          actionItemTask = actionItemTask.ParentTask ?? actionItemTask.ParentAssignment.Task;
          
          if (Sungero.RecordManagement.DocumentReviewTasks.Is(actionItemTask) && !actionItemTask.AccessRights.CanUpdate(performer))
          {
            actionItemTask.AccessRights.Grant(performer, DefaultAccessRightsTypes.Change);
            actionItemTask.AccessRights.Save();
          }
        }
      }
    }

    public virtual void PrepareDraftActionItemAssignmentBlockCompleteAssignment(GD.MainEDMS.IPrepareDraftActionItemAssignment assignment)
    {
      _obj.DraftActionItem = assignment.DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault();
      
      var executionAssignment =  GD.MainSolution.ActionItemExecutionAssignments.GetAll().FirstOrDefault(j => Equals(j.Task, _obj.ParentTask) &&
                                                                                                        j.Status == Sungero.Workflow.AssignmentBase.Status.InProcess &&
                                                                                                        Equals(j.Performer, Sungero.RecordManagement.ActionItemExecutionTasks.As(_obj.ParentTask).Assignee));
      
      if (executionAssignment != null && assignment.Result == MainSolution.PrepareDraftActionItemAssignment.Result.Explored)
        executionAssignment.Complete(Sungero.RecordManagement.ActionItemExecutionAssignment.Result.Done);
      else if (executionAssignment != null && assignment.Result != MainSolution.PrepareDraftActionItemAssignment.Result.SendForReview)
        executionAssignment.AssignmentStatusGD = GD.MainSolution.ActionItemExecutionAssignment.AssignmentStatusGD.InWorkGD;
      
      if (assignment.Result == MainSolution.PrepareDraftActionItemAssignment.Result.SendForExecute)
        GD.MainSolution.PublicFunctions.ActionItemExecutionTask.Remote.StartActionItemsFromDraft(GD.MainSolution.ActionItemExecutionTasks.As(_obj.ParentTask), assignment);
      
      // Рекурсивно прекратить подзадачи.
      if (assignment.NeedAbortChildActionItems ?? false)
        GD.MainSolution.Module.RecordManagement.PublicFunctions.Module.AbortSubtasksAndSendNoticesGD(GD.MainSolution.ActionItemExecutionTasks.As(_obj.ParentTask), assignment.Performer, GD.MainSolution.ActionItemExecutionTasks.Resources.AutoAbortingReason);
    }

    public virtual void PrepareDraftActionItemAssignmentBlockEnd(System.Collections.Generic.IEnumerable<GD.MainEDMS.IPrepareDraftActionItemAssignment> createdAssignments)
    {
      var assignment = createdAssignments.OrderByDescending(a => a.Created).FirstOrDefault();
      if (assignment != null && assignment.Result == MainSolution.PrepareDraftActionItemAssignment.Result.SendForExecute)
      {
        var actionItem = assignment.DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault() ?? _obj.DraftActionItem;
        GD.MainSolution.PublicFunctions.ActionItemExecutionTask.TransferEndBlockActionForExecution(GD.MainSolution.ActionItemExecutionTasks.As(_obj.ParentTask), MainSolution.ActionItemExecutionTasks.As(actionItem));
      }
    }
  }

}