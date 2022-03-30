using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.Condition;

namespace GD.MainSolution.Shared
{
  partial class ConditionFunctions
  {
    public override Sungero.Docflow.Structures.ConditionBase.ConditionResult CheckCondition(Sungero.Docflow.IOfficialDocument document, Sungero.Docflow.IApprovalTask task)
    {
      if (_obj.ConditionType == ConditionType.IsParallel)
      {
        var mainTask = ApprovalTasks.As(task);
        return Sungero.Docflow.Structures.ConditionBase.ConditionResult.Create(
          mainTask.IsParallelGD == true,
          string.Empty);
      }
      return base.CheckCondition(document, task);
    }
    
    public override System.Collections.Generic.Dictionary<string, List<Nullable<Enumeration>>> GetSupportedConditions()
    {
      var baseSupport = base.GetSupportedConditions();
      // Доступность типа условия "Параллельное согласование" для любого вида документа.
      var allTypes = Sungero.Docflow.PublicFunctions.DocumentKind.GetDocumentGuids(typeof(Sungero.Docflow.IOfficialDocument));
      foreach (var type in allTypes)
        baseSupport[type].Add(ConditionType.IsParallel);      
      return baseSupport;      
    }

  }
}