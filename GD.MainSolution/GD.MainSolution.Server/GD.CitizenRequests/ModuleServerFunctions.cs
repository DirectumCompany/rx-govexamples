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
    /// Сформировать отчет проект резолюции.
    /// </summary>
    /// <param name="task">Задача.</param>
    [Public]
    public virtual void AddDraftResolutionDocumentForExecution(Sungero.RecordManagement.IDocumentReviewAssignment assignment)
    {
      var actionItemExecutionTask = MainSolution.ActionItemExecutionTasks.As(assignment.Task);
      if (actionItemExecutionTask != null)
      {
        var actionItem = GD.GovernmentSolution.ActionItemExecutionTasks.As(assignment.ResolutionGroup.ActionItemExecutionTasks.FirstOrDefault());
        var report = Sungero.Docflow.OfficialDocuments.As(assignment.OtherGroup.All.FirstOrDefault());
        var addressee = Sungero.Company.PublicFunctions.Employee.Remote.GetEmployeeByName(assignment.Performer.Name);

        var document = AddDraftResolutionDocument(assignment.Task, actionItemExecutionTask.DocumentsGroup.OfficialDocuments.FirstOrDefault(), actionItem, report,  addressee);
        if (report == null)
        {
          actionItemExecutionTask.OtherGroup.All.Add(document);
          actionItemExecutionTask.Save();
        }
      }
    }
    /// <summary>
    /// Получить активное задание на исполнение поручений по документу.
    /// </summary>
    /// <param name="task">Задача на исполненеие.</param>
    /// <returns>Задание на приемку.</returns>
    [Public, Remote]
    public virtual MainSolution.IActionItemExecutionTask GetActualActionItemExecutionTask(MainSolution.IActionItemExecutionTask task)
    {
      return MainSolution.ActionItemExecutionTasks.GetAll()
        .Where(t => t.AttachmentDetails.Any(at => at.EntityTypeGuid == Guid.Parse(GD.CitizenRequests.PublicConstants.Request.RequestGuid) &&
                                            at.AttachmentId == task.DocumentsGroup.OfficialDocuments.FirstOrDefault().Id))
        .Where(t => (t.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.InProcess) ||
               (t.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.Draft) ||
               (t.Status == Sungero.RecordManagement.ActionItemExecutionTask.Status.Completed))
        .Where(t => Equals(t.AssignedBy, task.Assignee))
        .Where(t => t.IsTransferGD == true)
        .OrderByDescending(t => t.Created)
        .FirstOrDefault();
    }
  }
}