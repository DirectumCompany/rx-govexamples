using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.Condition;

namespace GD.MainSolution.Server
{
  partial class ConditionFunctions
  {
public override string GetConditionName()
    {
      using (TenantInfo.Culture.SwitchTo())
      {
        if (_obj.ConditionType == ConditionType.IsParallel)
          return GD.MainSolution.Conditions.Resources.IsParallel;        
      }
      return base.GetConditionName();
    }
  }
}