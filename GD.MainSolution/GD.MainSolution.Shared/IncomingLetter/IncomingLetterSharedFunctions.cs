using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.IncomingLetter;

namespace GD.MainSolution.Shared
{
  partial class IncomingLetterFunctions
  {
    public override void DocumentAttachedInMainGroup(Sungero.Workflow.ITask task)
    {
      base.DocumentAttachedInMainGroup(task);
      
      if (Sungero.RecordManagement.ActionItemExecutionTasks.Is(task))
      {
        var actionItem = Sungero.RecordManagement.ActionItemExecutionTasks.As(task);
        if (_obj.DeadlineGD != null)
        {
          if (actionItem.IsCompoundActionItem != true && actionItem.Deadline == null)
            actionItem.Deadline = _obj.DeadlineGD;
          else if (actionItem.IsCompoundActionItem == true && actionItem.FinalDeadline == null)
            actionItem.FinalDeadline = _obj.DeadlineGD;
        }
      }
    }

  }
}