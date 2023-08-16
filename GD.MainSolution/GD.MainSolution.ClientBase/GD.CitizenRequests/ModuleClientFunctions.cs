using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace GD.MainSolution.Module.CitizenRequests.Client
{
  partial class ModuleFunctions
  {
    /// <summary>
    /// Подписать документы для перенаправления.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <param name="actionItemExecution">Поручение.</param>
    /// <returns>Текст ошибки, если было исключение, иначе пустая строка.</returns>
    [Public]
    public virtual string SignatureTransferDocumentsForExecution(Sungero.Workflow.ITask task, IActionItemExecutionTask actionItemExecution)
    {
      var errorText = string.Empty;
      var currentTask = MainSolution.ActionItemExecutionTasks.As(task);
      if (currentTask != null)
      {
        if (GD.CitizenRequests.Requests.Is(currentTask.DocumentsGroup.OfficialDocuments.FirstOrDefault()))
        {
          if (actionItemExecution != null && GovernmentSolution.PublicFunctions.ActionItemExecutionTask.IsTransfer(actionItemExecution))
          {
            var documentList = new List<Sungero.Docflow.IOfficialDocument>();
            var notificationKind = Sungero.Docflow.PublicFunctions.DocumentKind.Remote.GetNativeDocumentKindRemote(GD.CitizenRequests.PublicConstants.Module.TransferNotificationKind);
            var notification = currentTask.CoverDocumentsGroup.OfficialDocuments.Where(d => Equals(d.DocumentKind, notificationKind)).FirstOrDefault();
            if (notification != null)
              documentList.Add(notification);
            var coveringLetterKind = Sungero.Docflow.PublicFunctions.DocumentKind.Remote.GetNativeDocumentKindRemote(GD.CitizenRequests.PublicConstants.Module.CoveringLetterKind);
            var letter = currentTask.CoverDocumentsGroup.OfficialDocuments.Where(d => Equals(d.DocumentKind, coveringLetterKind)).FirstOrDefault();
            if (letter != null)
              documentList.Add(letter);
            
            // Подписать документы.
            try
            {
              SignatureDocument(documentList, currentTask.AssignedBy);
            }
            catch (CommonLibrary.Exceptions.PlatformException ex)
            {
              Logger.Error("Failed to approve transfer documents.", ex);
              errorText = ex.Message;
            }
          }
        }
      }
      return errorText;
    }
  }
}