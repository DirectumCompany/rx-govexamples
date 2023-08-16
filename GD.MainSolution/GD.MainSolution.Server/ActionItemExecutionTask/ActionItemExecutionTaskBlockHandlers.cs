using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using Sungero.Company;
using GD.MainSolution.ActionItemExecutionTask;

namespace GD.MainSolution.Server.ActionItemExecutionTaskBlocks
{
  partial class ExecuteActionItemBlockHandlers
  {

    public override void ExecuteActionItemBlockCompleteAssignment(Sungero.RecordManagement.IActionItemExecutionAssignment assignment)
    {
      if (Functions.ActionItemExecutionTask.GetSecretary(Employees.As(assignment.Performer)) != null)
      {
        Logger.DebugFormat("ExecuteActionItemBlockCompleteAssignment with secretaries as task id {0}", _obj.Id.ToString());
        // Переписка.
        _obj.Report = assignment.ActiveText;
        
        // Завершить задание на продление срока, если оно есть.
        var extendDeadlineTasks = Sungero.Docflow.DeadlineExtensionTasks.GetAll(j => Equals(j.ParentAssignment, assignment) &&
                                                                                j.Status == Sungero.Workflow.Task.Status.InProcess);
        foreach (var extendDeadlineTask in extendDeadlineTasks)
          extendDeadlineTask.Abort();
        
        // Завершить задание на продление срока, если оно есть.
        var newExtendDeadlineTasks = Sungero.Docflow.DeadlineExtensionTasks.GetAll(j => Equals(j.ParentAssignment, assignment) &&
                                                                                   j.Status == Sungero.Workflow.Task.Status.InProcess);
        foreach (var newExtendDeadlineTask in newExtendDeadlineTasks)
          newExtendDeadlineTask.Abort();
        
        // Завершить задание на запрос отчёта, если оно есть.
        var reportRequestTasks = Sungero.RecordManagement.StatusReportRequestTasks.GetAll(r => Equals(r.ParentTask, assignment.Task) &&
                                                                                          r.Status == Sungero.Workflow.Task.Status.InProcess);
        foreach (var reportRequestTask in reportRequestTasks)
          reportRequestTask.Abort();
        
        // Рекурсивно прекратить подзадачи.
        if (assignment.NeedAbortChildActionItems ?? false)
          MainSolution.Module.RecordManagement.PublicFunctions.Module.AbortSubtasksAndSendNoticesGD(_obj, assignment.Performer, ActionItemExecutionTasks.Resources.AutoAbortingReason);
        
        // Выдать права на вложенные документы.
        Functions.ActionItemExecutionTask.GrantRightsToAttachmentsGD(_obj, _obj.ResultGroup.All.ToList(), false);
        
        // Связать документы из группы "Результаты исполнения" с основным документом.
        var mainDocument = _obj.DocumentsGroup.OfficialDocuments.FirstOrDefault();
        if (mainDocument != null)
        {
          foreach (var document in _obj.ResultGroup.OfficialDocuments.Where(d => !Equals(d, mainDocument)))
          {
            document.Relations.AddFrom(Sungero.RecordManagement.Constants.Module.SimpleRelationRelationName, mainDocument);
            document.Save();
          }
        }
      }
      else
      {
        Logger.DebugFormat("ExecuteActionItemBlockCompleteAssignment without secretaries as task id {0}", _obj.Id.ToString());
        base.ExecuteActionItemBlockCompleteAssignment(assignment);
      }
      
      // Выполнить задание на подготовку проекта резолюции помощником руководителя.
      var assistant = Functions.ActionItemExecutionTask.GetSecretary(Employees.As(assignment.Performer));
      var prepareDraft = PrepareDraftActionItemAssignments.GetAll(x => Equals(x.Task, assignment.Task) &&
                                                                    Equals(x.Performer, assistant) &&
                                                                    x.Status == GD.MainSolution.PrepareDraftActionItemAssignment.Status.InProcess).FirstOrDefault();
      if (prepareDraft != null)
        prepareDraft.Complete(GD.MainSolution.PrepareDraftActionItemAssignment.Result.Explored);
      
      // Прекратить задания на рассмотрение проекта резолюции руководителем.
      var reviewDraft = DocumentReviewAssignments.GetAll(a => Equals(a.Task, assignment.Task) &&
                                                         a.Status == GD.MainSolution.DocumentReviewAssignment.Status.InProcess).FirstOrDefault();
      
      if (reviewDraft != null)
        reviewDraft.Abort();
    }

    public override void ExecuteActionItemBlockEnd(System.Collections.Generic.IEnumerable<Sungero.RecordManagement.IActionItemExecutionAssignment> createdAssignments)
    {
      base.ExecuteActionItemBlockEnd(createdAssignments);
    }

    public override void ExecuteActionItemBlockStartAssignment(Sungero.RecordManagement.IActionItemExecutionAssignment assignment)
    {
      if (Functions.ActionItemExecutionTask.GetSecretary(Employees.As(assignment.Performer)) != null)
      {
        ActionItemExecutionAssignments.As(assignment).AssignmentStatusGD = GD.MainSolution.ActionItemExecutionAssignment.AssignmentStatusGD.PrepareDraftGD;
        assignment.IsRead = true;
      }
      base.ExecuteActionItemBlockStartAssignment(assignment);
      
      // Снять признак корректировки если нет помощника.
      _obj.WasCorrectionsGD = false;
      _obj.Save();
    }
  }

  partial class PrepareDraftActionItemAssignmentGDHandlers
  {

    public virtual void PrepareDraftActionItemAssignmentGDEnd(System.Collections.Generic.IEnumerable<GD.MainSolution.IPrepareDraftActionItemAssignment> createdAssignments)
    {
      var assignment = createdAssignments.OrderByDescending(a => a.Created).FirstOrDefault();
      if (assignment.Result == MainSolution.PrepareDraftActionItemAssignment.Result.SendForExecute)
      {
        var actionItem = assignment.DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault() ?? _obj.DraftActionItemGD;
        MainSolution.Functions.ActionItemExecutionTask.TransferEndBlockActionForExecution(_obj, MainSolution.ActionItemExecutionTasks.As(actionItem));
      }
    }

    public virtual void PrepareDraftActionItemAssignmentGDCompleteAssignment(GD.MainSolution.IPrepareDraftActionItemAssignment assignment)
    {
      _obj.DraftActionItemGD = assignment.DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault();
      
      var executionAssignment =  ActionItemExecutionAssignments.GetAll().FirstOrDefault(j => Equals(j.Task, _obj) &&
                                                                                        j.Status == Sungero.Workflow.AssignmentBase.Status.InProcess &&
                                                                                        Equals(j.Performer, _obj.Assignee));
      
      if (_obj.WasCorrectionsGD != true && executionAssignment != null && assignment.Result == MainSolution.PrepareDraftActionItemAssignment.Result.Explored)
        executionAssignment.Complete(Sungero.RecordManagement.ActionItemExecutionAssignment.Result.Done);
      else if (executionAssignment != null && assignment.Result != MainSolution.PrepareDraftActionItemAssignment.Result.SendForReview)
        executionAssignment.AssignmentStatusGD = GD.MainSolution.ActionItemExecutionAssignment.AssignmentStatusGD.InWorkGD;
      
      if (assignment.Result == MainSolution.PrepareDraftActionItemAssignment.Result.SendForExecute)
        GD.MainSolution.PublicFunctions.ActionItemExecutionTask.Remote.StartActionItemsFromDraft(ActionItemExecutionTasks.As(assignment.Task), assignment);
      
      // Рекурсивно прекратить подзадачи.
      if (assignment.NeedAbortChildActionItems ?? false)
        GD.MainSolution.Module.RecordManagement.PublicFunctions.Module.AbortSubtasksAndSendNoticesGD(_obj, assignment.Performer, ActionItemExecutionTasks.Resources.AutoAbortingReason);
    }

    public virtual void PrepareDraftActionItemAssignmentGDStartAssignment(GD.MainSolution.IPrepareDraftActionItemAssignment assignment)
    {
      var executionAssignment =  ActionItemExecutionAssignments.GetAll().FirstOrDefault(j => Equals(j.Task, _obj) &&
                                                                                        j.Status == Sungero.Workflow.AssignmentBase.Status.InProcess &&
                                                                                        Equals(j.Performer, _obj.Assignee));
      if (executionAssignment != null)
        executionAssignment.AssignmentStatusGD = GD.MainSolution.ActionItemExecutionAssignment.AssignmentStatusGD.ReviewDraftGD;
      
      if (_obj.DraftActionItemGD != null)
      {
        assignment.DraftActionItemGroup.ActionItemExecutionTasks.Clear();
        assignment.DraftActionItemGroup.ActionItemExecutionTasks.Add(_obj.DraftActionItemGD);
      }
      
      var document = _obj.DocumentsGroup.OfficialDocuments.FirstOrDefault();
      var performer = assignment.Performer;
      
      if (document != null)
      {
        Sungero.Docflow.PublicFunctions.OfficialDocument.GrantAccessRightsToActionItemAttachment(document, Sungero.Company.Employees.As(performer));
        Sungero.Docflow.PublicFunctions.Module.SynchronizeAddendaAndAttachmentsGroup(_obj.AddendaGroup, document);
      }      
      
      // Выдать права на основную задачу на рассмотрение.
      if (DocumentReviewTasks.Is(_obj.MainTask) && !_obj.MainTask.AccessRights.CanUpdate(performer))
      {
        _obj.MainTask.AccessRights.Grant(performer, DefaultAccessRightsTypes.Change);
        
        if (!_obj.MainTask.AccessRights.CanUpdate(performer))
          _obj.MainTask.AccessRights.Grant(performer, DefaultAccessRightsTypes.Change);
        
        _obj.MainTask.AccessRights.Save();
      }
      else
      {
        Sungero.Workflow.ITask currentTask = _obj;
        
        while (currentTask.ParentTask != null || currentTask.ParentAssignment != null)
        {
          currentTask = currentTask.ParentTask ?? currentTask.ParentAssignment.Task;
          
          if (DocumentReviewTasks.Is(currentTask) && !currentTask.AccessRights.CanUpdate(performer))
          {
            currentTask.AccessRights.Grant(performer, DefaultAccessRightsTypes.Change);
            currentTask.AccessRights.Save();
          }
        }
      }
    }
    
    public virtual void PrepareDraftActionItemAssignmentGDStart()
    {
      // Снять признак корректировки если есть помощник.
      _obj.WasCorrectionsGD = false;
      _obj.Save();
    }
  }
}