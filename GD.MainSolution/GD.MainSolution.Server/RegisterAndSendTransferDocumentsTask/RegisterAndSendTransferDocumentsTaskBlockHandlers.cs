using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using GD.MainSolution.RegisterAndSendTransferDocumentsTask;

namespace GD.MainSolution.Server.RegisterAndSendTransferDocumentsTaskBlocks
{
  partial class RegisterAndSendHandlers
  {

    public override void RegisterAndSendEnd(System.Collections.Generic.IEnumerable<GD.CitizenRequests.IRegisterAndSendTransferDocumentsAssignment> createdAssignments)
    {
      // Обработка перенаправления.
      var assignment = createdAssignments.OrderByDescending(a => a.Created).FirstOrDefault();
      // Если результат выполнения равен "Выполнен".
      if (assignment.Result == CitizenRequests.RegisterAndSendTransferDocumentsAssignment.Result.Complete)
      {
        var taskReview = GovernmentSolution.DocumentReviewTasks.As(_obj.MainTask);
        if (taskReview != null)
        {
          var actionItemTask = taskReview.ResolutionGroup.ActionItemExecutionTasks.Any() ?
            GovernmentSolution.ActionItemExecutionTasks.As(taskReview.ResolutionGroup.ActionItemExecutionTasks.FirstOrDefault()) :
            CitizenRequests.PublicFunctions.Module.Remote.GetActualActionItemExecutionTaskByReview(taskReview);
          if (actionItemTask != null)
            GovernmentSolution.PublicFunctions.ActionItemExecutionTask.SetDocumentStates(actionItemTask);
        }
        else
        {
          var taskItem = MainSolution.ActionItemExecutionTasks.As(_obj.MainTask);
          var actionItemTask = taskItem.DraftActionItemGD != null ? GovernmentSolution.ActionItemExecutionTasks.As(taskItem.DraftActionItemGD) :
            GD.MainSolution.Module.CitizenRequests.PublicFunctions.Module.Remote.GetActualActionItemExecutionTask(taskItem);          
          if (actionItemTask != null)
            GovernmentSolution.PublicFunctions.ActionItemExecutionTask.SetDocumentStates(actionItemTask);
        }
      }
    }

  }
}