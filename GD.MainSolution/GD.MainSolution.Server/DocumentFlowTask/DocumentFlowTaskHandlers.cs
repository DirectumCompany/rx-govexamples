using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.DocumentFlowTask;

namespace GD.MainSolution
{
  partial class DocumentFlowTaskServerHandlers
  {

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      var document = OfficialDocuments.As(_obj.DocumentGroup.ElectronicDocuments.FirstOrDefault());
      if (document != null && Functions.DocumentFlowTask.ContainsRegisterBlock(_obj) && !PublicFunctions.OfficialDocument.ExistsRegistrationSetting(document))
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