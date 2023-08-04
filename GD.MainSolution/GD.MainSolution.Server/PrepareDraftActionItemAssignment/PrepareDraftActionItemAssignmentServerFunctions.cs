using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.PrepareDraftActionItemAssignment;

namespace GD.MainSolution.Server
{
  partial class PrepareDraftActionItemAssignmentFunctions
  {
    [Public, Remote]
    public virtual Sungero.RecordManagement.IActionItemExecutionTask CreateActionItem()
    {
      var task = Sungero.RecordManagement.ActionItemExecutionTasks.Null;

      AccessRights.AllowRead(
        () =>
        {
          var rootTask = ActionItemExecutionTasks.As(_obj.Task);
          var executionAssignment =  ActionItemExecutionAssignments.GetAll().FirstOrDefault(j => Equals(j.Task, rootTask) &&
                                                                                            j.Status == Sungero.Workflow.AssignmentBase.Status.InProcess &&
                                                                                            Equals(j.Performer, rootTask.Assignee));
          if (executionAssignment != null)
          {
            var document = rootTask.DocumentsGroup.OfficialDocuments.FirstOrDefault();
            task = document == null ? GD.MainSolution.Module.RecordManagement.PublicFunctions.Module.Remote.CreateActionItemExecutionWithoutDoc(executionAssignment) :
              Sungero.RecordManagement.PublicFunctions.Module.Remote.CreateActionItemExecution(document, executionAssignment);
            task.Assignee = null;
            task.CoAssignees.Clear();
            if (executionAssignment.Deadline.HasValue)
              task.MaxDeadline = executionAssignment.Deadline.Value;
            task.IsDraftResolution = true;
            var assignedBy = rootTask.Assignee;
            task.AssignedBy = Sungero.Docflow.PublicFunctions.Module.Remote.IsUsersCanBeResolutionAuthor(document, assignedBy) ? assignedBy : null;

            if (executionAssignment.Deadline.HasValue &&
                (executionAssignment.Deadline.Value.HasTime() && executionAssignment.Deadline >= Calendar.Now ||
                 !executionAssignment.Deadline.Value.HasTime() && executionAssignment.Deadline >= Calendar.Today))
              task.Deadline = executionAssignment.Deadline;
          }

        });

      return task;
    }
  }
}