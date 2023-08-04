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
    /// Обработать перенаправление.
    /// </summary>
    /// <param name="task">Задача на испролнение поручения.</param>
    /// <param name="assignment">Задание.</param>
    [Public]
    public virtual void TransferEndBlockActionForExecution(MainSolution.IActionItemExecutionTask task, MainSolution.IPrepareDraftActionItemAssignment assignment)
    {
      var document = currentTask.DocumentsGroup.OfficialDocuments.FirstOrDefault();
      if (CitizenRequests.Requests.Is(document))
      {
        // Синхронизировать пункты поручения в вопросы обращения.
        var actionItem = assignment.DraftActionItemGroup.ActionItemExecutionTasks.Any() ?
          MainSolution.ActionItemExecutionTasks.As(assignment.DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault()) :
          MainSolution.PublicFunctions.Module. Remote.GetActualActionItemExecutionTask(currentTask);
        if (actionItem != null)
        {
          if (GovernmentSolution.PublicFunctions.ActionItemExecutionTask.IsTransfer(actionItem))
          {
            CitizenRequests.PublicFunctions.Module.Remote.StartSynchronizeResolutionToRequest(CitizenRequests.Requests.As(document), actionItem);
            // Отправить задачу на отправку и регистрацию писем по перенаправлению.
            var coverLetterKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(GD.CitizenRequests.PublicConstants.Module.CoveringLetterKind);
            var coverLetter = currentTask.CoverDocumentsGroup.OfficialDocuments.Where(d => Equals(d.DocumentKind, coverLetterKind)).FirstOrDefault();
            var notificationKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(GD.CitizenRequests.PublicConstants.Module.TransferNotificationKind);
            var notification = currentTask.CoverDocumentsGroup.OfficialDocuments.Where(d => Equals(d.DocumentKind, notificationKind)).FirstOrDefault();
            CitizenRequests.PublicFunctions.Module.Remote.StartRegisterAndSendTransferDocument(CitizenRequests.Requests.As(document),
                                                                                               CitizenRequests.OutgoingRequestLetters.As(coverLetter),
                                                                                               CitizenRequests.OutgoingRequestLetters.As(notification), currentTask);
          }
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
    /// Удалить проекты резолюции из заданий на подготовку и рассмотрение проекта резолюции
    /// </summary>
    /// <param name="assignment">Задание исполнителю</param>
    /// <param name="performer">Исполнитель</param>
    /// <param name="deadline">Новый срок</param>
    public void ForwardAssignmentGD(IAssignment assignment, IUser performer, DateTime? deadline)
    {

      //Перенаправить поручение на нового исполнителя и зачитстиь область вложений "Проект резолюции"
      if (GD.MainSolution.ActionItemExecutionAssignments.Is(assignment))
      {
        _obj.DraftActionItemGD = null;
        _obj.WasCorrectionsGD = true;
        _obj.Save();
        
        //Найти все задания на подготовку проекта резолюции и удалить из областей вложений проект резолюции
        var allPrepareDraftAI = PrepareDraftActionItemAssignments.GetAll(x => Equals(x.Task, assignment.Task));
        
        foreach (IPrepareDraftActionItemAssignment prepareDraftAI in allPrepareDraftAI)
        {
          prepareDraftAI.DraftActionItemGroup.ActionItemExecutionTasks.Clear();
          prepareDraftAI.Save();
        }
        
        //Найти все задания на рассмотрение проекта резолюции и удалить из областей вложений проект резолюции
        var allReviewDraftAI = DocumentReviewAssignments.GetAll(x => Equals(x.Task, assignment.Task));
        foreach (IDocumentReviewAssignment reviewDraftAI in allReviewDraftAI)
        {
          reviewDraftAI.ResolutionGroup.ActionItemExecutionTasks.Clear();
          reviewDraftAI.Save();
        }
        
        //Прекратить задаине старому исполнителю
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
    /// Проверка сроков исолнителя и соисполнителя перед стартом проекта поручения
    /// </summary>
    [Public, Remote]
    public string CheckDeadlineInResolution()
    {
      if (Sungero.RecordManagement.PublicFunctions.ActionItemExecutionTask.CheckOverdueActionItemExecutionTask(_obj))
        return GD.MainSolution.ActionItemExecutionTasks.Resources.PerformerDeadlineLessThenTodayCorrectIt;
      else if ((_obj.CoAssignees.Any() &&
                _obj.CoAssigneesDeadline != null &&
                ((_obj.CoAssigneesDeadline.Value.HasTime() &&
                  _obj.CoAssigneesDeadline < Calendar.Now) ||
                 (!_obj.CoAssigneesDeadline.Value.HasTime() &&
                  _obj.CoAssigneesDeadline < Calendar.Now.Date))) ||
               _obj.ActionItemParts.Any(x => x.CoAssigneesDeadline != null &&
                                        ((x.CoAssigneesDeadline.Value.HasTime() &&
                                          x.CoAssigneesDeadline < Calendar.Now) ||
                                         (!x.CoAssigneesDeadline.Value.HasTime() &&
                                          x.CoAssigneesDeadline < Calendar.Now.Date))))
        return GD.MainSolution.ActionItemExecutionTasks.Resources.CoexecutorDeadlineLessThenTodayCorrectIt;
      return string.Empty;
    }
  }
}