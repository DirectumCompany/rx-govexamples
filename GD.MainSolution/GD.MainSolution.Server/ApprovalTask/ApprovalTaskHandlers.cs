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
      base.BeforeStart(e);
      
      if (Functions.ApprovalTask.ContainsRegisterStage(_obj) && !Functions.ApprovalTask.ExistsRegistrationSetting(_obj))
      {
        e.AddError(GD.MainSolution.ApprovalTasks.Resources.RegistrationSettingNotFoundError);
      }
      
      var actionItem = Functions.ApprovalTask.GetActionItemFromIncomingLetter(_obj);
      if (actionItem != null)
        _obj.ActiveText = ApprovalTasks.Resources.ApprovalTextWithActionItemFormat(Sungero.Core.Hyperlinks.Get(actionItem));
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      if (!_obj.State.IsCopied)
        _obj.IsParallelGD = false;
    }
  }

}