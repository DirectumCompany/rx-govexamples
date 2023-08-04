using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace GD.MainSolution.Module.CitizenRequests.Server
{
  partial class ModuleFunctions
  {
    /// <summary>
    /// Получить активное задание на исполнение поручений по документу.
    /// </summary>
    /// <param name="task">Задача на исполненеие.</param>
    /// <returns>Задание на приемку.</returns>
    [Public, Remote]
    public virtual GovernmentSolution.IActionItemExecutionTask GetActualActionItemExecutionTask(MainSolution.IActionItemExecutionTask task)
    {
      return MainSolution.ActionItemExecutionTasks.GetAll()
        .Where(t => t.AttachmentDetails.Any(at => at.EntityTypeGuid == Guid.Parse(GD.CitizenRequests.PublicConstants.Request.RequestGuid) &&
                                            at.AttachmentId == task.DocumentsGroup.OfficialDocuments.FirstOrDefault().Id))
        .Where(t => (t.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.InProcess) ||
               (t.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.Draft) ||
               (t.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.Completed))
        .Where(t => Equals(t.AssignedBy, task.Addressee))
        .Where(t => t.IsTransferGD == true)
        .OrderByDescending(t => t.Created)
        .FirstOrDefault();
    }
  }
}