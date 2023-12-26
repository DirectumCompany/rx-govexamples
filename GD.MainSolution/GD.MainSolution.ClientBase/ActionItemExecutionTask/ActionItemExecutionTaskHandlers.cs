using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionTask;

namespace GD.MainSolution
{
  partial class ActionItemExecutionTaskClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      _obj.State.Attachments.CoverDocumentsGroup.IsVisible = _obj.CoverDocumentsGroup.All.Any();      
    }
  }

  partial class ActionItemExecutionTaskActionItemPartsClientHandlers
  {

    public override void ActionItemPartsAssigneeValueInput(Sungero.RecordManagement.Client.ActionItemExecutionTaskActionItemPartsAssigneeValueInputEventArgs e)
    {
      base.ActionItemPartsAssigneeValueInput(e);
      if (e.NewValue == null)
        _obj.BusinessUnitGD = null;
      
      if (e.NewValue != null && e.NewValue.Department != null)
        _obj.BusinessUnitGD = e.NewValue.Department.BusinessUnit;
    }

    public virtual void ActionItemPartsBusinessUnitGDValueInput(GD.MainSolution.Client.ActionItemExecutionTaskActionItemPartsBusinessUnitGDValueInputEventArgs e)
    {
      if (e.NewValue != null && (_obj.Assignee == null || _obj.Assignee.Department != null && !Equals(_obj.Assignee.Department.BusinessUnit, e.NewValue)))
        _obj.Assignee = e.NewValue.CEO;
    }
  }
}