using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionTask;

namespace GD.MainSolution.Client
{
  partial class ActionItemExecutionTaskActions
  {
    public override void ChangeTransferActionItemGD(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.ChangeTransferActionItemGD(e);
    }

    public override bool CanChangeTransferActionItemGD(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
       return (_obj.State.IsInserted || Locks.GetLockInfo(_obj).IsLockedByMe) &&
        _obj.Status == Sungero.Workflow.Task.Status.Draft &&
        (_obj.IsDraftResolution == true || CitizenRequests.Requests.Is(_obj.DocumentsGroup.OfficialDocuments.FirstOrDefault());
    }

  }

}