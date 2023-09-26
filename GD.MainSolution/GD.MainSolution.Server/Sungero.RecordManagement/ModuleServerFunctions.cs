using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace GD.MainSolution.Module.RecordManagement.Server
{
  partial class ModuleFunctions
  {
    // Непубличная функция превращается в публичную.
    /// <summary>
    /// Рекурсивно завершить все подзадачи, выслать уведомления.
    /// </summary>
    /// <param name="actionItem">Поручение, подзадачи которого следует завершить.</param>
    /// <param name="performer">Исполнитель, которого не нужно уведомлять.</param>
    /// <param name="abortingReason">Причина прекращения.</param>
    [Public]
    public static void AbortSubtasksAndSendNoticesGD(IActionItemExecutionTask actionItem, IUser performer = null, string abortingReason = "")
    {
      AbortSubtasksAndSendNotices(actionItem, performer, abortingReason);
    }
    
    /// <summary>
    /// Создать поручение без документа с указанием задания-основания.
    /// </summary>
    /// <param name="parentAssignment">Задание-основание.</param>
    /// <returns>Поручение.</returns>
    /// <remarks>Для создания подпоручения, явно указывать MainTask.
    /// Необходимо для корректной работы вложений и текстов в задаче.
    /// Метод !!ПАДАЕТ если задание имеет в своих вложениях document. Баг 24899.</remarks>
    [Remote(PackResultEntityEagerly = true), Public]
    public static IActionItemExecutionTask CreateActionItemExecutionWithoutDoc(Sungero.Workflow.IAssignment parentAssignment)
    {
      var task = parentAssignment == null ? ActionItemExecutionTasks.Create() : ActionItemExecutionTasks.CreateAsSubtask(parentAssignment);
      task.Subject = MainSolution.Functions.ActionItemExecutionTask.GetActionItemExecutionSubjectGD(task, ActionItemExecutionTasks.Resources.TaskSubject);
      
      if (parentAssignment != null)
      {
        foreach (var otherGroupAttachment in parentAssignment.AllAttachments)
          task.OtherGroup.All.Add(otherGroupAttachment);
      }
      
      return task;
    }
    
    // Копия DeleteActionItemExecutionTasks.
    /// <summary>
    /// Удалить поручения.
    /// </summary>
    /// <param name="actionItems">Список поручений.</param>
    [Public]
    public virtual void DeleteActionItemExecutionTasks(List<IActionItemExecutionTask> actionItems)
    {
      foreach (var draftResolution in actionItems)
        ActionItemExecutionTasks.Delete(draftResolution);
    }
  }
}