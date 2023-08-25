using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionTask;
using System.Text;

namespace GD.MainSolution.Client
{
  partial class ActionItemExecutionTaskFunctions
  {
    /// <summary>
    /// Создать диалог для формирования сопроводительных документов.
    /// </summary>
    /// <param name="actionItemExecution">Поручение.</param>
    /// <param name="errorLetter">Ошибки для сопроводительного письма.</param>
    /// <param name="errorNotice">Ошибки для уведомления.</param>
    /// <param name="eventArgs">Аргумент обработчика вызова.</param>
    /// <returns>True, если диалог был, иначе false.</returns>
    public virtual bool CreateTransferDocumentsForExecution(IActionItemExecutionTask actionItemExecution, string errorCoverLetter, string errorNotification,  Sungero.Domain.Client.ExecuteActionArgs eventArgs)
    {
      // Сформировать текст диалога.
      var dialogText = new StringBuilder();
      if (!string.IsNullOrEmpty(errorCoverLetter))
        dialogText.AppendLine(errorCoverLetter);
      if (!string.IsNullOrEmpty(errorNotification))
        dialogText.AppendLine(errorNotification);
      dialogText.AppendLine();
      dialogText.AppendLine(GD.CitizenRequests.Resources.GenerateCoverLetters);
      
      var dialog = Dialogs.CreateTaskDialog(GD.CitizenRequests.Resources.NecessaryToGenerateCoverLetters,
                                            dialogText.ToString(),
                                            MessageType.Question);
      var onReviewButton = dialog.Buttons.AddCustom(GD.CitizenRequests.Resources.GenerateTransferLetter);
      var cancelButton = dialog.Buttons.AddCancel();
      var result = dialog.Show();
      if (result == onReviewButton)
      {
        var isProblemCreatingCoverLetter = false;
        var isProblemCreatingNotification = false;
        
        // Сформировать/переформировать сопроводительное письмо и уведомление.
        if (!string.IsNullOrEmpty(errorCoverLetter))
        {
          var coverLetter = CreateCoverLetterForExecution(actionItemExecution, eventArgs);
          if (coverLetter != null && !_obj.CoverDocumentsGroup.OfficialDocuments.Contains(coverLetter))
          {
            _obj.CoverDocumentsGroup.OfficialDocuments.Add(coverLetter);
            _obj.Save();
          }
          isProblemCreatingCoverLetter = coverLetter == null;
        }
        
        if (!string.IsNullOrEmpty(errorNotification))
        {
          var notificationTransfer = CreateTransferNotificationForExecution(actionItemExecution, eventArgs);
          if (notificationTransfer != null && !_obj.CoverDocumentsGroup.OfficialDocuments.Contains(notificationTransfer))
          {
            _obj.CoverDocumentsGroup.OfficialDocuments.Add(notificationTransfer);
            _obj.Save();
          }
          isProblemCreatingNotification = notificationTransfer == null;
        }
        return !isProblemCreatingCoverLetter && !isProblemCreatingNotification;
      }
      
      else
        return false;
    }
    
    /// <summary>
    /// Создать сопроводительное письмо.
    /// </summary>
    /// <param name="actionItemExecution">Поручение.</param>
    /// <param name="eventArgs">Аргумент обработчика вызова.</param>
    /// <returns>Сопроводительное письмо.</returns>
    public virtual CitizenRequests.IOutgoingRequestLetter CreateCoverLetterForExecution(MainSolution.IActionItemExecutionTask actionItemExecution, Sungero.Domain.Client.ExecuteActionArgs eventArgs)
    {
      var coveringLetterKind = Sungero.Docflow.PublicFunctions.DocumentKind.Remote.GetNativeDocumentKindRemote(CitizenRequests.PublicConstants.Module.CoveringLetterKind);
      var letter = CitizenRequests.OutgoingRequestLetters.As(_obj.CoverDocumentsGroup.OfficialDocuments.Where(d => Equals(d.DocumentKind, coveringLetterKind)).FirstOrDefault());
      var request = CitizenRequests.Requests.As(_obj.DocumentsGroup.OfficialDocuments.FirstOrDefault());
      
      var errorText = CitizenRequests.PublicFunctions.Module.CheckCreateCoverLetter(letter, request, actionItemExecution);
      if (!string.IsNullOrEmpty(errorText))
      {
        eventArgs.AddError(errorText);
        return null;
      }
      var coverLetter = CitizenRequests.PublicFunctions.Module.Remote.CreateOrUpdateCoverLetter(actionItemExecution, letter);
      if (coverLetter != null)
        Dialogs.NotifyMessage(GD.CitizenRequests.Resources.TransferCoverLetterGeneratedSuccessfully);
      
      return coverLetter;
    }
    
    /// <summary>
    /// Создать уведомление о перенаправлении.
    /// </summary>
    /// <param name="actionItemExecution">Поручение.</param>
    /// <param name="eventArgs">Аргумент обработчика вызова.</param>
    /// <returns>Уведомление заявителю.</returns>
    public virtual CitizenRequests.IOutgoingRequestLetter CreateTransferNotificationForExecution(MainSolution.IActionItemExecutionTask actionItemExecution, Sungero.Domain.Client.ExecuteActionArgs eventArgs)
    {
      var notificationKind = Sungero.Docflow.PublicFunctions.DocumentKind.Remote.GetNativeDocumentKindRemote(CitizenRequests.PublicConstants.Module.TransferNotificationKind);
      var notification = CitizenRequests.OutgoingRequestLetters.As(_obj.CoverDocumentsGroup.OfficialDocuments.Where(d => Equals(d.DocumentKind, notificationKind)).FirstOrDefault());
      
      var request = CitizenRequests.Requests.As(_obj.DocumentsGroup.OfficialDocuments.FirstOrDefault());
      var errorText = CitizenRequests.PublicFunctions.Module.CheckCreateNotification(notification, request, actionItemExecution);
      if (!string.IsNullOrEmpty(errorText))
      {
        eventArgs.AddError(errorText);
        return null;
      }
      
      var notificationTransfer = CitizenRequests.PublicFunctions.Module.Remote.CreateOrUpdateTransferNotification(actionItemExecution, notification);
      if (notificationTransfer != null)
        Dialogs.NotifyMessage(GD.CitizenRequests.Resources.NotificationTransferSuccessfullyGenerated);
      
      return notificationTransfer;
    }
    
    /// <summary>
    /// Проверить актуальность сопроводительного письма и уведомления.
    /// </summary>
    /// <param name="resolution">Задача на исполнение поручений.</param>
    /// <param name="eventArgs">Аргумент обработчика вызова.</param>
    /// <returns>True, если нет ошибок или документы перегенерили, иначе false.</returns>
    public virtual bool CheckActualityLettersForExecution(MainSolution.IActionItemExecutionTask resolution, Sungero.Domain.Client.ExecuteActionArgs eventArgs)
    {
      var coveringLetterKind = Sungero.Docflow.PublicFunctions.DocumentKind.Remote.GetNativeDocumentKindRemote(GD.CitizenRequests.PublicConstants.Module.CoveringLetterKind);
      var notificationKind = Sungero.Docflow.PublicFunctions.DocumentKind.Remote.GetNativeDocumentKindRemote(GD.CitizenRequests.PublicConstants.Module.TransferNotificationKind);
      
      var letter = CitizenRequests.OutgoingRequestLetters.As(_obj.CoverDocumentsGroup.OfficialDocuments.Where(d => Equals(d.DocumentKind, coveringLetterKind)).FirstOrDefault());
      var notification = CitizenRequests.OutgoingRequestLetters.As(_obj.CoverDocumentsGroup.OfficialDocuments.Where(d => Equals(d.DocumentKind, notificationKind)).FirstOrDefault());

      // Проверить актуальность сопроводительного письма.
      var errorCoverLetter = CitizenRequests.PublicFunctions.Module.CheckActualityCoverLetter(resolution, letter);
      // Проверить актуальность уведомления.
      var errorNotification = CitizenRequests.PublicFunctions.Module.CheckActualityNotification(resolution, notification);
      
      // Вызвать диалог.
      if (!string.IsNullOrEmpty(errorCoverLetter) || !string.IsNullOrEmpty(errorNotification))
        return CreateTransferDocumentsForExecution(resolution, errorCoverLetter, errorNotification, eventArgs);
      else
        return true;
      
    }
  }
}