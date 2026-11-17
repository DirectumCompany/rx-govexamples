using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionTask;

namespace GD.MainSolution
{
  partial class ActionItemExecutionTaskCreatingFromServerHandler
  {

    public override void CreatingFrom(Sungero.Domain.CreatingFromEventArgs e)
    {
      base.CreatingFrom(e);
      e.Without(_info.Properties.DraftActionItemGD);
    }
  }

  partial class ActionItemExecutionTaskServerHandlers
  {

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      // Заглушка для обхода падения при отправке в работу проекта, подготовленного в рамках задачи на исполнение поручения.
      if (_obj.IsDraftResolution == true && !_obj.DocumentsGroup.OfficialDocuments.Any() && ActionItemExecutionTasks.Is(_obj.ParentAssignment.Task))
        _obj.IsDraftResolution = false;
      
      base.BeforeStart(e);
    }
  }

}