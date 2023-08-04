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
      if (Sungero.Company.PublicFunctions.Employee.GetManagerAssistants(_obj.Assignee).Any(x => x.PreparesResolution == true))
      {
        Logger.DebugFormat("!!! CompleteAssignment4 with secretaries as task id {0}", _obj.Id.ToString());
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
        Logger.DebugFormat("!!! CompleteAssignment4 without secretaries as task id {0}", _obj.Id.ToString());
        base.ExecuteActionItemBlockCompleteAssignment(assignment);
      }
      
      //Выполнить задание на подготовку проекта резолюции помощником руководителя.
      var assistant = Sungero.Docflow.PublicFunctions.Module.GetSecretary(Employees.As(assignment.Performer));
      var prepareDraftAI = PrepareDraftActionItemAssignments.GetAll(x => Equals(x.Task, assignment.Task) &&
                                                                    Equals(x.Performer, assistant) &&
                                                                    x.Status == GD.MainSolution.PrepareDraftActionItemAssignment.Status.InProcess).FirstOrDefault();
      if (prepareDraftAI != null)
        prepareDraftAI.Complete(GD.MainSolution.PrepareDraftActionItemAssignment.Result.Explored);
      
      //Прекратить задания на рассмотрение проекта резолюции руководителем.
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
      var secretary = Sungero.Company.PublicFunctions.Employee.GetManagerAssistantsWhoPrepareDraftResolution(_obj.Assignee);
      
      if (secretary.Any())
      {
        ActionItemExecutionAssignments.As(assignment).AssignmentStatusGD = GD.MainSolution.ActionItemExecutionAssignment.AssignmentStatusGD.PrepareDraftGD;
      }
      
      base.ExecuteActionItemBlockStartAssignment(assignment);
      
      //Снять признак корректировки если нет помощника.
      _obj.WasCorrectionsGD = false;
      _obj.Save();
    }

    public override void ExecuteActionItemBlockStart()
    {
      Logger.DebugFormat("Задача на исполнение поручения. Старт блока. ИД задачи = {0} до", _obj.Id);
      base.ExecuteActionItemBlockStart();
      Logger.DebugFormat("Задача на исполнение поручения. Старт блока. ИД задачи = {0} после", _obj.Id);
    }
  }

  partial class PrepareDraftActionItemAssignmentGDHandlers
  {

    public virtual void PrepareDraftActionItemAssignmentGDEnd(System.Collections.Generic.IEnumerable<GD.MainSolution.IPrepareDraftActionItemAssignment> createdAssignments)
    {
      var assignment = createdAssignments.OrderByDescending(a => a.Created).FirstOrDefault();
      if (assignment.Result == MainSolution.PrepareDraftActionItemAssignment.Result.SendForExecute)
        MainSolution.Functions.ActionItemExecutionTask.TransferEndBlockActionForExecution(_obj, assignment);
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
    public static Sungero.Company.IEmployee GetSecretary(Sungero.Company.IEmployee manager)
    {
      return Sungero.Company.PublicFunctions.Employee.GetManagerAssistants(manager).Where(x => x.PreparesResolution == true).Select(m => m.Assistant).FirstOrDefault();
    }

    public virtual void PrepareDraftActionItemAssignmentGDStartAssignment(GD.MainSolution.IPrepareDraftActionItemAssignment assignment)
    {
      Logger.DebugFormat("Задача на исполнение поручения. Старт задания подготовка проекта резолюции. ИД задачи = {0} до", _obj.Id);
      
      var executionAssignment =  ActionItemExecutionAssignments.GetAll().FirstOrDefault(j => Equals(j.Task, _obj) &&
                                                                                        j.Status == Sungero.Workflow.AssignmentBase.Status.InProcess &&
                                                                                        Equals(j.Performer, _obj.Assignee));
      if (executionAssignment != null)
        executionAssignment.AssignmentStatusGD = GD.MainSolution.ActionItemExecutionAssignment.AssignmentStatusGD.ReviewDraftGD;
      
      if (_obj.DraftActionItemGD != null )
      {
        assignment.DraftActionItemGroup.ActionItemExecutionTasks.Clear();
        assignment.DraftActionItemGroup.ActionItemExecutionTasks.Add(_obj.DraftActionItemGD);
      }
      
      Logger.DebugFormat("Задача на исполнение поручения. Старт задания подготовка проекта резолюции. ИД задачи = {0} после", _obj.Id);
    }

    public virtual void PrepareDraftActionItemAssignmentGDStart()
    {
      Logger.DebugFormat("Задача на исполнение поручения. Старт блока подготовка проекта резолюции. ИД задачи = {0} до", _obj.Id);
      var assignee = _obj.Assignee;
      var assistant = GetSecretary(assignee);
      
      if (assistant != null)
      {
        // Добавить помощника в качестве исполнителя.
        _block.Performers.Add(assistant);
        
        // Вычислить срок задания.
        // На подготовку проекта поручения 4 часа.
        _block.RelativeDeadlineHours = 4;
        
        var document = _obj.DocumentsGroup.OfficialDocuments.FirstOrDefault();
        
        // Проставляем признак того, что задание для доработки.
        var lastReview = Assignments
          .GetAll(a => Equals(a.Task, _obj) && Equals(a.TaskStartId, _obj.StartId))
          .OrderByDescending(a => a.Created)
          .FirstOrDefault();
        var actionItemSubject = document != null ? document.Name : _obj.ActionItem;
        var subject = string.Empty;
        if (lastReview != null && Sungero.RecordManagement.DocumentReviewAssignments.Is(lastReview) &&
            lastReview.Result == Sungero.RecordManagement.DocumentReviewAssignment.Result.DraftResRework)
        {
          subject = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(GD.MainSolution.ActionItemExecutionTasks.Resources.ReworkPrepareDraftActionItem, actionItemSubject);
        }
        else
        {
          subject = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(GD.MainSolution.ActionItemExecutionTasks.Resources.PrepareDraftActionItem, actionItemSubject);
        }
        
        _block.Subject = subject.Substring(0, subject.Length > 250 ? 250 : subject.Length);
        if (document != null)
        {
          Sungero.Docflow.PublicFunctions.OfficialDocument.GrantAccessRightsToActionItemAttachment(document, assistant);
          Sungero.Docflow.PublicFunctions.Module.SynchronizeAddendaAndAttachmentsGroup(_obj.AddendaGroup, document);
        }
        
        GD.MainSolution.Functions.ActionItemExecutionTask.GrantRightsToAttachments(_obj, assistant);
        
        // Выдать права на основную задачу на рассмотрение.
        if (DocumentReviewTasks.Is(_obj.MainTask) && !_obj.MainTask.AccessRights.CanUpdate(assignee))
        {
          _obj.MainTask.AccessRights.Grant(assignee, DefaultAccessRightsTypes.Change);
          
          if (!_obj.MainTask.AccessRights.CanUpdate(assistant))
            _obj.MainTask.AccessRights.Grant(assistant, DefaultAccessRightsTypes.Change);
          
          _obj.MainTask.AccessRights.Save();
        }
        else
        {
          Sungero.Workflow.ITask currentTask = _obj;
          
          while (currentTask.ParentTask != null || currentTask.ParentAssignment != null)
          {
            currentTask = currentTask.ParentTask ?? currentTask.ParentAssignment.Task;
            
            if (DocumentReviewTasks.Is(currentTask) && !currentTask.AccessRights.CanUpdate(assistant))
            {
              currentTask.AccessRights.Grant(assistant, DefaultAccessRightsTypes.Change);
              currentTask.AccessRights.Save();
            }
          }
        }
        
        //Снять признак корректировки если есть помощник.
        _obj.WasCorrectionsGD = false;
        _obj.Save();
      }
      
      Logger.DebugFormat("Задача на исполнение поручения. Старт блока подготовка проекта резолюции. ИД задачи = {0} после", _obj.Id);
    }
  }
}