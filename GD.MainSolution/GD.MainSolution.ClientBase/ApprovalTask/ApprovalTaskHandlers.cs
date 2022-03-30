using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ApprovalTask;

namespace GD.MainSolution
{
  partial class ApprovalTaskClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      _obj.State.Properties.IsParallelGD.IsVisible = _obj.ApprovalRule != null && _obj.ApprovalRule.Conditions.Any(q => q.Condition.ConditionType == GD.MainSolution.Condition.ConditionType.IsParallel);      
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
    }

  }
}