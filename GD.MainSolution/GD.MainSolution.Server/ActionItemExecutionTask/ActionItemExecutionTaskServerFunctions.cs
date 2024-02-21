using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Workflow;
using GD.MainSolution.ActionItemExecutionTask;

namespace GD.MainSolution.Server
{
  partial class ActionItemExecutionTaskFunctions
  {

    /// <summary>
    /// Обновление Исполнителей и соисполнителей в задаче на рассмотрение поручения.
    /// </summary>
    [Public]
    public void PerformersUpdate()
    {
      var performers = string.Empty;
      if (_obj.IsCompoundActionItem == true)
        performers = string.Join("; ", _obj.ActionItemParts.Select(x => x.Assignee.Person.ShortName).ToArray());
      else
      {
        performers = _obj.Assignee.Person.ShortName + "; ";
        if (_obj.CoAssignees.Any())
          performers += string.Join("; ", _obj.CoAssignees.Select(x => x.Assignee.Person.ShortName).ToArray());
      }
      if (!string.IsNullOrEmpty(performers) && performers.Length > 250)
        performers = performers.Substring(0, 250);
      _obj.PerformersGD = performers;
    }
    
    /// <summary>
    /// После выполнения ведущего задания на исполнение поручения заполнить в нем свойство "Выполнил" исполнителем задания.
    /// </summary>
    [Public, Remote]
    public override void SetCompletedByInParentAssignment()
    {
      var assignment = Sungero.RecordManagement.PublicFunctions.ActionItemExecutionTask.GetParentAssignment(_obj);
      if (assignment != null)
      {
        var currentUser = Users.Current;
        var performer = assignment.Performer;
        if (assignment.Status == Sungero.Workflow.Assignment.Status.Completed &&
            currentUser != null && currentUser.IsSystem == true && Equals(currentUser, assignment.CompletedBy))
        {
          Logger.DebugFormat("ActionItemExecutionAssignment(ID={0}) performer: {1}(ID={2}).", assignment.Id, performer.DisplayValue, performer.Id);
          Logger.DebugFormat("ActionItemExecutionAssignment(ID={0}) completed by {1}(ID={2}). Set CompletedBy to {3}(ID={4}).",
                             assignment.Id,
                             currentUser.DisplayValue, currentUser.Id,
                             performer.DisplayValue, performer.Id);
          assignment.CompletedBy = performer;
          assignment.Save();
        }
      }
    }
    
    /// <summary>
    /// Проверить возможность изменения поручения перед показом диалога корректировки.
    /// </summary>
    /// <returns>Текст ошибки или пустую строку, если ошибок нет.</returns>
    [Remote(IsPure = true)]
    public override string CheckActionItemEditBeforeDialog()
    {
      var checkResult = base.CheckActionItemEditBeforeDialog();
      if (checkResult == null)
      {
        // Найти все задания на рассмотрение проекта резолюции.
        var allReviewDraft = DocumentReviewAssignments.GetAll(x => Equals(x.Task, _obj));
        foreach (IDocumentReviewAssignment reviewDraft in allReviewDraft)
        {
          var lockInfo = Locks.GetLockInfo(reviewDraft);
          if (lockInfo.IsLocked)
            checkResult = GD.MainSolution.ActionItemExecutionTasks.Resources.DraftOrderIsBlockedFormat(lockInfo.OwnerName);

        }
      }
      return checkResult;
    }
    
    /// <summary>
    /// Выдать права на сопроводительные документы.
    /// </summary>
    /// <param name="assignees">Исполнители.</param>
    public virtual void GrantAccessRightsOnCoverDocument(List<IRecipient> assignees)
    {
      var coverDocuments = _obj.CoverDocumentsGroup.OfficialDocuments;
      if (coverDocuments.Any())
      {
        foreach (var assignee in assignees)
        {
          foreach (var document in coverDocuments)
          {
            Sungero.Docflow.PublicFunctions.Module.GrantAccessRightsOnEntity(document, assignee, DefaultAccessRightsTypes.Change);
          }
        }
      }
    }
    /// <summary>
    /// Получить тему по умолчанию для задания "Подготовка проекта поручения".
    /// </summary>
    /// <param name="task">Задача на исполнение поручения.</param>
    /// <returns>Тема для задания "Подготовка проекта поручения".</returns>
    [ExpressionElement("PrepareDraftActionItemAssignmentDefaultSubject", "")]
    public static string GetPrepareDraftActionItemAssignmentDefaultSubject(IActionItemExecutionTask task)
    {
      var subject = task.DraftActionItemGD != null ? ActionItemExecutionTasks.Resources.FinalizeDraftOrderActionItem : ActionItemExecutionTasks.Resources.PrepareDraftOrder;
      return Sungero.RecordManagement.Shared.ActionItemExecutionTaskFunctions.GetActionItemExecutionSubject(task, subject);
    }
    
    /// <summary>
    /// Вернуть помощника по задаче на исполнение поручения, который готовит проект резолюции.
    /// </summary>
    /// <param name="task">Задача на исполнение поручения.</param>
    /// <returns>Помощник.</returns>
    [ExpressionElement("AssistantPreparesActionItem", "")]
    public static Sungero.Company.IEmployee GetAssistantsWhoPrepareDraftResolution(IActionItemExecutionTask task)
    {
      return Sungero.Company.PublicFunctions.Employee.GetManagerAssistantsWhoPrepareDraftResolution(task.Assignee).Select(m => m.Assistant).FirstOrDefault();
    }
    
    /// <summary>
    /// Получить тему по умолчанию для задания "Рассмотрение проекта поручения".
    /// </summary>
    /// <param name="task">Задача на исполнение поручения.</param>
    /// <returns>Тема для задания "Рассмотрение проекта поручения".</returns>
    [ExpressionElement("DocumentReviewAssignmentDefaultSubject", "")]
    public static string GetDocumentReviewAssignmentDefaultSubject(IActionItemExecutionTask task)
    {
      return Sungero.RecordManagement.Shared.ActionItemExecutionTaskFunctions.GetActionItemExecutionSubject(task, ActionItemExecutionTasks.Resources.ReviewDraftActionItem);
    }
    
    /// <summary>
    /// Обработать перенаправление.
    /// </summary>
    /// <param name="actionItem">Поручение.</param>
    [Public]
    public virtual void TransferEndBlockActionForExecution(IActionItemExecutionTask actionItem)
    {
      var document = _obj.DocumentsGroup.OfficialDocuments.FirstOrDefault();
      if (CitizenRequests.Requests.Is(document))
      {
        // Синхронизировать пункты поручения в вопросы обращения.
        if (actionItem != null && GovernmentSolution.PublicFunctions.ActionItemExecutionTask.IsTransfer(actionItem))
        {
          CitizenRequests.PublicFunctions.Module.Remote.StartSynchronizeResolutionToRequest(CitizenRequests.Requests.As(document), actionItem);
          // Отправить задачу на отправку и регистрацию писем по перенаправлению.
          var coverLetterKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(GD.CitizenRequests.PublicConstants.Module.CoveringLetterKind);
          var coverLetter = _obj.CoverDocumentsGroup.OfficialDocuments.Where(d => Equals(d.DocumentKind, coverLetterKind)).FirstOrDefault();
          var notificationKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(GD.CitizenRequests.PublicConstants.Module.TransferNotificationKind);
          var notification = _obj.CoverDocumentsGroup.OfficialDocuments.Where(d => Equals(d.DocumentKind, notificationKind)).FirstOrDefault();
          CitizenRequests.PublicFunctions.Module.Remote.StartRegisterAndSendTransferDocument(CitizenRequests.Requests.As(document),
                                                                                             CitizenRequests.OutgoingRequestLetters.As(coverLetter),
                                                                                             CitizenRequests.OutgoingRequestLetters.As(notification), _obj);
        }
      }
    }
    /// <summary>
    /// Получить незавершенные подчиненные поручения по ведущему заданию.
    /// </summary>
    /// <param name="entity"> Поручение, для которого требуется получить незавершенные.</param>
    /// <returns>Список незавершенных подчиненных поручений.</returns>
    [Remote(IsPure = true)]
    public static List<IActionItemExecutionTask> GetSubActionItemExecutions(IActionItemExecutionAssignment entity)
    {
      // TODO Котегов: есть бага платформы 19850.
      return ActionItemExecutionTasks.GetAll()
        .Where(t => entity != null && t.ParentAssignment == entity)
        .Where(t => t.ActionItemType == Sungero.RecordManagement.ActionItemExecutionTask.ActionItemType.Additional ||
               t.ActionItemType == Sungero.RecordManagement.ActionItemExecutionTask.ActionItemType.Main)
        .Where(t => t.Status.Value == Sungero.Workflow.Task.Status.InProcess)
        .ToList();
    }
    
    /// <summary>
    /// Переадресовать задание новому исполнителю и попытаться прекратить задание старому.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    /// <param name="performer">Новый исполнитель.</param>
    /// <remarks>Если "старое задание" заблокировано, то будет выполнена только переадресация,
    /// а прекращение будет в рамках схемы подзадачи соисполнителю.</remarks>
    public override void ForwardAssignment(IAssignment assignment, IUser performer)
    {
      this.ForwardAssignmentGD(assignment, performer, null);
    }
    
    /// <summary>
    /// Переадресовать задание новому исполнителю и попытаться прекратить задание старому.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    /// <param name="performer">Новый исполнитель.</param>
    /// <param name="deadline">Новый срок.</param>
    /// <remarks>Если "старое задание" заблокировано, то будет выполнена только переадресация,
    /// а прекращение будет в рамках схемы подзадачи соисполнителю.</remarks>
    public override void ForwardAssignment(IAssignment assignment, IUser performer, DateTime? deadline)
    {
      this.ForwardAssignmentGD(assignment, performer, deadline);
    }
    
    /// <summary>
    /// Переадресовать задание новому исполнителю и попытаться прекратить задание старому.
    /// Удалить проекты резолюции из заданий на подготовку и рассмотрение проекта резолюции.
    /// </summary>
    /// <param name="assignment">Задание исполнителю</param>
    /// <param name="performer">Исполнитель</param>
    /// <param name="deadline">Новый срок</param>
    public void ForwardAssignmentGD(IAssignment assignment, IUser performer, DateTime? deadline)
    {

      // Перенаправить поручение на нового исполнителя и зачитстиь область вложений "Проект резолюции".
      if (GD.MainSolution.ActionItemExecutionAssignments.Is(assignment))
      {
        _obj.DraftActionItemGD = null;
        _obj.WasCorrectionsGD = true;
        _obj.Save();
        
        // Найти все задания на подготовку проекта резолюции и удалить из областей вложений проект резолюции.
        var allPrepareDraft = PrepareDraftActionItemAssignments.GetAll(x => Equals(x.Task, assignment.Task));
        
        foreach (IPrepareDraftActionItemAssignment prepareDraft in allPrepareDraft)
        {
          prepareDraft.DraftActionItemGroup.ActionItemExecutionTasks.Clear();
          prepareDraft.Save();
        }
        
        // Найти все задания на рассмотрение проекта резолюции и удалить из областей вложений проект резолюции.
        var allReviewDraft = DocumentReviewAssignments.GetAll(x => Equals(x.Task, assignment.Task));
        foreach (IDocumentReviewAssignment reviewDraft in allReviewDraft)
        {
          reviewDraft.ResolutionGroup.ActionItemExecutionTasks.Clear();
          reviewDraft.Save();
        }
        
        // Прекратить задаине старому исполнителю.
        if (!Locks.GetLockInfo(assignment).IsLocked)
          assignment.Abort();
      }
      else
      {
        if (deadline != null)
          assignment.Forward(performer, deadline);
        else
          assignment.Forward(performer);
        
        if (!Locks.GetLockInfo(assignment).IsLocked)
          assignment.Abort();
      }
    }
    
    /// <summary>
    /// Выдать права на вложения поручения.
    /// </summary>
    /// <param name="attachmentGroup"> Группа вложения.</param>
    /// <param name="needGrantAccessRightsToPerformer"> Нужно ли выдать права исполнителю.</param>
    public virtual void GrantRightsToAttachmentsGD(List<Sungero.Domain.Shared.IEntity> attachmentGroup, bool needGrantRightToPerformer)
    {
      GrantAccessRightsToAttachments(attachmentGroup, needGrantRightToPerformer);
    }
    
    /// <summary>
    /// Выдать права на вложения поручения.
    /// </summary>
    /// <param name="employee">Работник, которому необходимо выдать права на вложения.</param>
    public virtual void GrantRightsToAttachments(Sungero.Company.IEmployee employee)
    {
      foreach (var item in _obj.AddendaGroup.All.Where(x => Sungero.Content.ElectronicDocuments.Is(x)))
      {
        var accessRights = item.AccessRights;
        if (employee != null && !accessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Read, employee))
        {
          accessRights.Grant(employee, DefaultAccessRightsTypes.Read);
          accessRights.Save();
        }
      }
      
      foreach (var item in _obj.OtherGroup.All.Where(x => Sungero.Content.ElectronicDocuments.Is(x)))
      {
        var accessRights = item.AccessRights;
        if (employee != null && !accessRights.IsGrantedDirectly(DefaultAccessRightsTypes.Read, employee))
        {
          accessRights.Grant(employee, DefaultAccessRightsTypes.Read);
          accessRights.Save();
        }
      }
    }
    
    /// <summary>
    /// Отправить проект резолюции на исполнение.
    /// </summary>
    /// <param name="parentAssignment">Задание на рассмотрение.</param>
    [Remote, Public]
    public void StartActionItemsFromDraft(Sungero.Workflow.IAssignment parentAssignment)
    {
      var draftResolution = _obj.DraftActionItemGD;
      if (draftResolution == null && PrepareDraftActionItemAssignments.Is(parentAssignment))
        draftResolution = PrepareDraftActionItemAssignments.As(parentAssignment).DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault();
      
      if (draftResolution == null || draftResolution.Status != Sungero.Workflow.Task.Status.Draft)
        return;
      
      draftResolution.Save();
      ((Sungero.Domain.Shared.IExtendedEntity)draftResolution).Params[Sungero.RecordManagement.PublicConstants.ActionItemExecutionTask.CheckDeadline] = true;
      draftResolution.Start();
    }
    
    /// <summary>
    /// Проверка сроков исолнителя и соисполнителя перед стартом проекта поручения.
    /// </summary>
    [Public, Remote]
    public string CheckDeadlineInResolution()
    {
      if (Sungero.RecordManagement.PublicFunctions.ActionItemExecutionTask.CheckOverdueActionItemExecutionTask(_obj))
        return GD.MainSolution.ActionItemExecutionTasks.Resources.PerformerDeadlineLessThenTodayCorrectIt;
      else if ((_obj.CoAssignees.Any() && _obj.CoAssigneesDeadline != null &&
                ((_obj.CoAssigneesDeadline.Value.HasTime() && _obj.CoAssigneesDeadline < Calendar.Now) ||
                 (!_obj.CoAssigneesDeadline.Value.HasTime() && _obj.CoAssigneesDeadline < Calendar.Now.Date))) ||
               _obj.ActionItemParts.Any(x => x.CoAssigneesDeadline != null && ((x.CoAssigneesDeadline.Value.HasTime() &&  x.CoAssigneesDeadline < Calendar.Now) ||
                                                                               (!x.CoAssigneesDeadline.Value.HasTime() && x.CoAssigneesDeadline < Calendar.Now.Date))))
        return GD.MainSolution.ActionItemExecutionTasks.Resources.CoexecutorDeadlineLessThenTodayCorrectIt;
      return string.Empty;
    }
    
    public static Sungero.Company.IEmployee GetSecretary(Sungero.Company.IEmployee manager)
    {
      return Sungero.Company.PublicFunctions.Employee.GetManagerAssistantsWhoPrepareDraftResolution(manager).Select(m => m.Assistant).FirstOrDefault();
    }
  }
}