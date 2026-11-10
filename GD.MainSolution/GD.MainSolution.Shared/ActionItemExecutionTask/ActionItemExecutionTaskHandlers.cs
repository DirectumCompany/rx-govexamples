using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionTask;

namespace GD.MainSolution
{
  partial class ActionItemExecutionTaskSharedHandlers
  {

    public override void AssignedByChanged(Sungero.RecordManagement.Shared.ActionItemExecutionTaskAssignedByChangedEventArgs e)
    {
      base.AssignedByChanged(e);
      if (e.NewValue != null && e.NewValue != e.OldValue)
      {
        var settings = Sungero.Docflow.PublicFunctions.PersonalSetting.GetPersonalSettings(e.NewValue);
        _obj.IsAutoExec = settings != null && (_obj.IsUnderControl != true || !Equals(_obj.Supervisor, e.NewValue))
          ? settings.IsAutoExecLeadingActionItem
          : false;
      }
    }
  }
}