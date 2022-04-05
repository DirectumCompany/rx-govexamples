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
      e.CloseFormAfterAction = true;
      base.CreateChildActionItem(e);
    }

    public override bool CanCreateChildActionItem(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateChildActionItem(e);
    }

  }

}