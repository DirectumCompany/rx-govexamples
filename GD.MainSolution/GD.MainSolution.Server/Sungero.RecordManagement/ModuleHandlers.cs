using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace GD.MainSolution.Module.RecordManagement.Server
{
  partial class ForExecutionFolderHandlers
  {

    public override IQueryable<Sungero.Workflow.IAssignmentBase> ForExecutionDataQuery(IQueryable<Sungero.Workflow.IAssignmentBase> query)
    {
      var result = base.ForExecutionDataQuery(query);
      
      if (_filter?.ShowAssistantAssignmentGD == false)
        result = result.Where(a => Sungero.RecordManagement.ActionItemExecutionAssignments.Is(a) &&
                              (MainSolution.ActionItemExecutionAssignments.As(a).AssignmentStatusGD == null ||
                               MainSolution.ActionItemExecutionAssignments.As(a).AssignmentStatusGD == GD.MainSolution.ActionItemExecutionAssignment.AssignmentStatusGD.InWorkGD)||
                              !Sungero.RecordManagement.ActionItemExecutionAssignments.Is(a));
      
      return result;
    }
  }

  partial class RecordManagementHandlers
  {
  }
}