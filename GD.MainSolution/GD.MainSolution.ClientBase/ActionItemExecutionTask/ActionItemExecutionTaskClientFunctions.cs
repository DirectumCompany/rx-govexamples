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
      dialogText.AppendLine(Environment.NewLine);
      dialogText.AppendLine(GD.CitizenRequests.Resources.GenerateCoverLetters);
      
      var dialog = Dialogs.CreateTaskDialog(GD.CitizenRequests.Resources.NecessaryToGenerateCoverLetters,
                                            dialogText.ToString(),
                                            MessageType.Question);
      var onReviewButton = dialog.Buttons.AddCustom(GD.CitizenRequests.Resources.GenerateTransferLetter);
      var cancelButton = dialog.Buttons.AddCancel();
      var result = dialog.Show();
      if (result == onReviewButton)
      {
        // Сформировать/переформировать сопроводительное письмо и уведомление.
        if (!string.IsNullOrEmpty(errorCoverLetter))
          CreateCoverLetterForExecution(eventArgs);
        if (!string.IsNullOrEmpty(errorNotification))
          CreateTransferNotificationForExecution(eventArgs);
        return true;
      }
      else
        return false;
    }
    
    /// <summary>
    /// Создать сопроводительное письмо.
    /// </summary>
    /// <param name="actionItemExecution">Поручение.</param>
    /// <param name="eventArgs">Аргумент обработчика вызова.</param>
    public virtual void CreateCoverLetterForExecution(Sungero.Domain.Client.ExecuteActionArgs eventArgs)
    {
      var coveringLetterKind = Sungero.Docflow.PublicFunctions.DocumentKind.Remote.GetNativeDocumentKindRemote(CitizenRequests.PublicConstants.Module.CoveringLetterKind);
      var letter = CitizenRequests.OutgoingRequestLetters.As(_obj.CoverDocumentsGroup.OfficialDocuments.Where(d => Equals(d.DocumentKind, coveringLetterKind)).FirstOrDefault());
      var request = CitizenRequests.Requests.As(_obj.DocumentsGroup.OfficialDocuments.FirstOrDefault());
      
      var errorText = CitizenRequests.PublicFunctions.Module.CheckCreateCoverLetter(letter, request, _obj);
      if (!string.IsNullOrEmpty(errorText))
      {
        eventArgs.AddError(errorText);
        return;
      }
      var coverLetter = CitizenRequests.PublicFunctions.Module.Remote.CreateOrUpdateCoverLetter(_obj, letter);
      if (coverLetter != null)
      {
        if (!_obj.CoverDocumentsGroup.OfficialDocuments.Contains(coverLetter))
        {
          _obj.CoverDocumentsGroup.OfficialDocuments.Add(coverLetter);
          _obj.Save();
        }
        Dialogs.NotifyMessage(GD.CitizenRequests.Resources.TransferCoverLetterGeneratedSuccessfully);
      }
    }
    
    /// <summary>
    /// Создать уведомление о перенаправлении.
    /// </summary>
    /// <param name="actionItemExecution">Поручение.</param>
    /// <param name="eventArgs">Аргумент обработчика вызова.</param>
    public virtual void CreateTransferNotificationForExecution(Sungero.Domain.Client.ExecuteActionArgs eventArgs)
    {
      var notificationKind = Sungero.Docflow.PublicFunctions.DocumentKind.Remote.GetNativeDocumentKindRemote(CitizenRequests.PublicConstants.Module.TransferNotificationKind);
      var notification = CitizenRequests.OutgoingRequestLetters.As(_obj.CoverDocumentsGroup.OfficialDocuments.Where(d => Equals(d.DocumentKind, notificationKind)).FirstOrDefault());
      
      var request = CitizenRequests.Requests.As(_obj.DocumentsGroup.OfficialDocuments.FirstOrDefault());
      var errorText = CitizenRequests.PublicFunctions.Module.CheckCreateNotification(notification, request, _obj);
      if (!string.IsNullOrEmpty(errorText))
      {
        eventArgs.AddError(errorText);
        return;
      }
      
      var  notificationTransfer = CitizenRequests.PublicFunctions.Module.Remote.CreateOrUpdateTransferNotification(_obj, notification);
      if (notificationTransfer != null)
      {
        if (!_obj.CoverDocumentsGroup.OfficialDocuments.Contains(notificationTransfer))
        {
          _obj.CoverDocumentsGroup.OfficialDocuments.Add(notificationTransfer);
          _obj.Save();
        }
        
        Dialogs.NotifyMessage(GD.CitizenRequests.Resources.NotificationTransferSuccessfullyGenerated);
      }
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