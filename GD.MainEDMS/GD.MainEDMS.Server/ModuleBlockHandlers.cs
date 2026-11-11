using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;

namespace GD.MainEDMS.Server.MainEDMSBlocks
{
  partial class PrepareDraftActionItemTaskBlockHandlers
  {

    public virtual void PrepareDraftActionItemTaskBlockEnd(System.Collections.Generic.IEnumerable<GD.MainEDMS.IPrepareDraftActionItemTask> createdTasks)
    {
      var task = createdTasks.FirstOrDefault();
      if (task != null)
        _block.OutProperties.OutDraftActionItemTask = GD.MainSolution.ActionItemExecutionTasks.As(task.DraftActionItem);
    }

    public virtual void PrepareDraftActionItemTaskBlockStartTask(GD.MainEDMS.IPrepareDraftActionItemTask task)
    {
      task.ParentActionItemTask = _block.ParentActionItemTask;
      task.DraftActionItem = _block.InDraftActionItemTask;
    }
  }

}