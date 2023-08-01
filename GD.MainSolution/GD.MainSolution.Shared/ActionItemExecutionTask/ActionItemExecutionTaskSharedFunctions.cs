using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionTask;

namespace GD.MainSolution.Shared
{
  partial class ActionItemExecutionTaskFunctions
  {
    public static string GetActionItemExecutionSubjectGD(IActionItemExecutionTask task, CommonLibrary.LocalizedString beginningSubject)
    {
      var subject = GetActionItemExecutionSubject(task, beginningSubject);
      return subject.Substring(0, subject.Length > 250 ? 250 : subject.Length);
    }
  }
}