using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ApprovalTask;

namespace GD.MainSolution.Server
{
  partial class ApprovalTaskFunctions
  {
    /// <summary>
    /// Проверить, есть ли в задаче на согласование по регламенту этап регистрации.
    /// </summary>
    /// <returns>true, если содержится. Иначе - false.</returns>
    public bool ContainsRegisterStage()
    {
      return GetStages(_obj).Stages.Any(s => s.StageType == Sungero.Docflow.ApprovalStage.StageType.Register);
    }
  }
}