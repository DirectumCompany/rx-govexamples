using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionAssignment;

namespace GD.MainSolution
{
  partial class ActionItemExecutionAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      base.Showing(e);
      
      if (_obj.AssignmentStatusGD == MainSolution.ActionItemExecutionAssignment.AssignmentStatusGD.PrepareDraftGD ||
           _obj.AssignmentStatusGD == MainSolution.ActionItemExecutionAssignment.AssignmentStatusGD.ReviewDraftGD)
      {
        e.AddInformation(GD.MainSolution.ActionItemExecutionAssignments.Resources.NoActionItemDraft);
        e.HideAction(_obj.Info.Actions.Done);
        e.HideAction(_obj.Info.Actions.CreateChildActionItem);
      }
    }
  }
}