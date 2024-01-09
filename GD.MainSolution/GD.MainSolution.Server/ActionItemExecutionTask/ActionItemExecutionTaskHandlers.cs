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
      if (_obj.IsDraftResolution == true && !_obj.DocumentsGroup.OfficialDocuments.Any() && ActionItemExecutionTasks.Is(_obj.ParentAssignment.Task))
        _obj.IsDraftResolution = false;
      
      base.BeforeStart(e);
      var performers = string.Empty;
      if (_obj.IsCompoundActionItem == true)
        performers = string.Join("; ", _obj.ActionItemParts.Select( x => x.Assignee.Person.ShortName).ToArray());
      else
      {
        performers = _obj.Assignee.Person.ShortName + "; ";
        if (_obj.CoAssignees.Any())
          performers += string.Join("; ", _obj.CoAssignees.Select( x => x.Assignee.Person.ShortName).ToArray());
      }
      if (!string.IsNullOrEmpty(performers) && performers.Length > 250)
        performers = performers.Substring(0, 250);
      _obj.PerformersGD = performers;  
    }
  }

}