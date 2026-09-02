using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ApprovalTask;

namespace GD.MainSolution
{
  partial class ApprovalTaskServerHandlers
  {

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      var document = OfficialDocuments.As(_obj.DocumentGroup.OfficialDocuments.FirstOrDefault());
      if (document != null && Functions.ApprovalTask.ContainsRegisterStage(_obj) && !PublicFunctions.OfficialDocument.ExistsRegistrationSetting(document))
        e.AddError(GD.MainSolution.ApprovalTasks.Resources.RegistrationSettingNotFoundError);
      
      base.BeforeStart(e);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      if (!_obj.State.IsCopied)
        _obj.IsParallelGD = false;
    }
  }

}