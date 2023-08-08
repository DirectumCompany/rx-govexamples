using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;

namespace GD.MainSolution.Module.RecordManagement.Server.RecordManagementBlocks
{
  partial class DocumentReviewBlockHandlers
  {

    public override void DocumentReviewBlockStart()
    {
      base.DocumentReviewBlockStart();
      
      var actionItemTask = ActionItemExecutionTasks.As(_obj);
      
      if (actionItemTask != null)
      {
        var document = actionItemTask.DocumentsGroup.OfficialDocuments.FirstOrDefault();
        
        if (document != null)
          Sungero.Docflow.PublicFunctions.Module.SynchronizeAddendaAndAttachmentsGroup(actionItemTask.AddendaGroup, document);
      }
    }

    public override void DocumentReviewBlockStartAssignment(Sungero.RecordManagement.IDocumentReviewAssignment assignment)
    {
      base.DocumentReviewBlockStartAssignment(assignment);
      
      var actionItemTask = ActionItemExecutionTasks.As(_obj);
      
      if (actionItemTask != null)
      {
        var executionAssignment =  ActionItemExecutionAssignments.GetAll().FirstOrDefault(j => Equals(j.Task, _obj) &&
                                                                                          j.Status == Sungero.Workflow.AssignmentBase.Status.InProcess &&
                                                                                          Equals(j.Performer, actionItemTask.Assignee));
        if (executionAssignment != null)
          executionAssignment.AssignmentStatusGD = GD.MainSolution.ActionItemExecutionAssignment.AssignmentStatusGD.ReviewDraftGD;
        
        if (actionItemTask.DraftActionItemGD != null )
        {
          assignment.ResolutionGroup.ActionItemExecutionTasks.Clear();
          assignment.ResolutionGroup.ActionItemExecutionTasks.Add(actionItemTask.DraftActionItemGD);
        }
      }
    }

    public override void DocumentReviewBlockCompleteAssignment(Sungero.RecordManagement.IDocumentReviewAssignment assignment)
    {
      base.DocumentReviewBlockCompleteAssignment(assignment);
      var executionAssignment =  ActionItemExecutionAssignments.GetAll().FirstOrDefault(j => Equals(j.Task, assignment.Task) &&
                                                                                        j.Status == Sungero.Workflow.AssignmentBase.Status.InProcess &&
                                                                                        Equals(j.Performer, ActionItemExecutionTasks.As(assignment.Task).Assignee));
      if (executionAssignment != null)
      {
        if (assignment.Result == DocumentReviewAssignment.Result.Informed)
        {
          executionAssignment.Complete(MainSolution.ActionItemExecutionAssignment.Result.Done);
          executionAssignment.AssignmentStatusGD = GD.MainSolution.ActionItemExecutionAssignment.AssignmentStatusGD.InWorkGD;
        }
        
        if (assignment.Result == DocumentReviewAssignment.Result.DraftResApprove)
        {
          GD.MainSolution.PublicFunctions.ActionItemExecutionTask.Remote.StartActionItemsFromDraft(ActionItemExecutionTasks.As(_obj), executionAssignment);
          executionAssignment.AssignmentStatusGD = GD.MainSolution.ActionItemExecutionAssignment.AssignmentStatusGD.InWorkGD;
        }
      }
    }
  }

}