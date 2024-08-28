using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionTask;

namespace GD.MainSolution
{
  partial class ActionItemExecutionTaskServerHandlers
  {

    public override void BeforeStart(Sungero.Workflow.Server.BeforeStartEventArgs e)
    {
      // Заглушка для обхода падения при отправке в работу проекта, подготовленного в рамках задачи на исполнение поручения.
      if (_obj.IsDraftResolution == true && !_obj.DocumentsGroup.OfficialDocuments.Any() && ActionItemExecutionTasks.Is(_obj.ParentAssignment.Task))
        _obj.IsDraftResolution = false;
      
      base.BeforeStart(e);
    }

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      if (!_obj.State.IsCopied)
        _obj.ActiveText = Sungero.RecordManagement.ActionItemExecutionTasks.Resources.DefaultActionItem;
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      PublicFunctions.ActionItemExecutionTask.PerformersUpdate(_obj);
    }
  }

}