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
      base.RegisterAndSendEnd(createdAssignments);
      
      var taskItem = MainSolution.ActionItemExecutionTasks.As(_obj.MainTask);
      if (taskItem != null)
      {
        var actionItemTask = taskItem.DraftActionItemGD != null ? GovernmentSolution.ActionItemExecutionTasks.As(taskItem.DraftActionItemGD) :
          GD.MainSolution.Module.CitizenRequests.PublicFunctions.Module.Remote.GetActualActionItemExecutionTask(taskItem);
        if (actionItemTask != null)
          GovernmentSolution.PublicFunctions.ActionItemExecutionTask.SetDocumentStates(actionItemTask);
      }

    }
  }
}