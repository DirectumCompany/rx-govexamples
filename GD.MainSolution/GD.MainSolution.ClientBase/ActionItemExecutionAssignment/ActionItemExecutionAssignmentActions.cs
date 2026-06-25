using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionAssignment;

namespace GD.MainSolution.Client
{
  partial class ActionItemExecutionAssignmentActions
  {
    public override void CreateChildActionItem(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.State.IsChanged)
        _obj.Save();
      // base.CreateChildActionItem(e);
      Logger.DebugFormat("ActionItemExecutionAssignment (ID={0}). Start CreateChildActionItem.", _obj.Id);
      var subTask = Functions.ActionItemExecutionTask.Remote.CreateActionItemExecutionFromExecution(ActionItemExecutionTasks.As(_obj.Task), _obj);
      subTask.ShowModal();
      Logger.DebugFormat("ActionItemExecutionAssignment (ID={0}). End CreateChildActionItem.", _obj.Id);
      if (subTask.Status == Sungero.Workflow.Task.Status.InProcess)
        e.CloseFormAfterAction = true;
    }

    public override bool CanCreateChildActionItem(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateChildActionItem(e);
    }

  }
  
}