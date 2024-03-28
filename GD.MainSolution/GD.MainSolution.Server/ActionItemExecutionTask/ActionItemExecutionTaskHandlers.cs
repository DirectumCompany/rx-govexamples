using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionTask;

namespace GD.MainSolution
{
  partial class ActionItemExecutionTaskServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      if (!_obj.State.IsCopied)
        _obj.ActiveText = Sungero.RecordManagement.ActionItemExecutionTasks.Resources.DefaultActionItem;
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      PublicFunctions.ActionItemExecutionTask.PerformersUpdate(_obj);
    }
  }

}