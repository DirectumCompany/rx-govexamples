using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.DocumentReviewAssignment;

namespace GD.MainSolution.Client
{
  partial class DocumentReviewAssignmentActions
  {
    public virtual void OpenActionItemGD(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      if (_obj.ResolutionGroup.ActionItemExecutionTasks.FirstOrDefault() != null)
        _obj.ResolutionGroup.ActionItemExecutionTasks.FirstOrDefault().ShowModal();
    }

    public virtual bool CanOpenActionItemGD(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

    public override void DraftResApprove(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (ActionItemExecutionTasks.Is(_obj.Task))
      {
        var draftActionItem = _obj.ResolutionGroup.ActionItemExecutionTasks.FirstOrDefault();
        var error = string.Empty;
        if (GD.MainSolution.ActionItemExecutionTasks.Is(draftActionItem))
          error = PublicFunctions.ActionItemExecutionTask.Remote.CheckDeadlineInResolution(GD.MainSolution.ActionItemExecutionTasks.As(draftActionItem));
        if (!string.IsNullOrEmpty(error))
        {
          e.AddError(error);
          e.Cancel();
        }
        
        var lockInfo = Locks.GetLockInfo(draftActionItem);
        if (draftActionItem != null && lockInfo != null && lockInfo.IsLocked)
        {
          e.AddError(GD.MainSolution.DocumentReviewAssignments.Resources.ActionItemLockedFormat(lockInfo.OwnerName),
                     _obj.Info.Actions.OpenActionItemGD);
          e.Cancel();
        }
      }
      else
        base.DraftResApprove(e);
    }

    public override bool CanDraftResApprove(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return base.CanDraftResApprove(e);
    }

    public override void DraftResRework(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      if (ActionItemExecutionTasks.Is(_obj.Task))
      {
        // Проверить наличие замечаний в тексте.
        if (string.IsNullOrWhiteSpace(_obj.ActiveText))
        {
          e.AddError(GD.MainSolution.DocumentReviewAssignments.Resources.NeedTextToRework);
          return;
        }
      }
      else
        base.DraftResRework(e);
    }

    public override bool CanDraftResRework(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return base.CanDraftResRework(e);
    }

    public override void Forward(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      base.Forward(e);
    }

    public override bool CanForward(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      var isActionItemExecutionTask = ActionItemExecutionTasks.Is(_obj.Task);
      return !isActionItemExecutionTask && base.CanForward(e);
    }

  }




}