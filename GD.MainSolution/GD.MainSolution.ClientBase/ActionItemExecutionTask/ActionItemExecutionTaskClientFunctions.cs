using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionTask;

namespace GD.MainSolution.Client
{
  partial class ActionItemExecutionTaskFunctions
  {
    /// <summary>
    /// Создать диалог для формирования сопроводительных документов.
    /// </summary>
    /// <param name="task">Задача.</param>
    /// <param name="errorLetter">Ошибки для сопроводительного письма.</param>
    /// <param name="errorNotice">Ошибки для уведомления.</param>
    /// <param name="eventArgs">Аргумент обработчика вызова.</param>
    /// <returns>True, если диалог был, иначе false.</returns>
    public virtual bool CreateTransferDocumentsForExecution(string errorCoverLetter, string errorNotification,  Sungero.Domain.Client.ExecuteActionArgs eventArgs)
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
    /// <param name="eventArgs">Аргумент обработчика вызова.</param>
    public virtual void CreateCoverLetterForExecution(Sungero.Domain.Client.ExecuteActionArgs eventArgs)
    {
      var coveringLetterKind = Sungero.Docflow.PublicFunctions.DocumentKind.Remote.GetNativeDocumentKindRemote(CitizenRequests.PublicConstants.Module.CoveringLetterKind);
      var letter = CitizenRequests.OutgoingRequestLetters.As(_obj.CoverDocumentsGroup.OfficialDocuments.Where(d => Equals(d.DocumentKind, coveringLetterKind)).FirstOrDefault());
      
      var request = CitizenRequests.Requests.As(_obj.DocumentsGroup.OfficialDocuments.FirstOrDefault());
      
      var resolution = _obj.Gro ActionItemExecutionTasks.Any() ?
        MainSolution.ActionItemExecutionTasks.As(_obj..ActionItemExecutionTasks.FirstOrDefault()) :
        MainSolution.Module.CitizenRequests.PublicFunctions.Module.Remote.GetActualActionItemExecutionTask(_obj);
      
      var errorText = CitizenRequests.PublicFunctions.Module.CheckCreateCoverLetter(letter, request, resolution);
      if (!string.IsNullOrEmpty(errorText))
      {
        eventArgs.AddError(errorText);
        return;
      }
      var coverLetter = CitizenRequests.PublicFunctions.Module.Remote.CreateOrUpdateCoverLetter(resolution, letter);
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
    /// <param name="eventArgs">Аргумент обработчика вызова.</param>
    public virtual void CreateTransferNotificationForExecution(Sungero.Domain.Client.ExecuteActionArgs eventArgs)
    {
      var notificationKind = Sungero.Docflow.PublicFunctions.DocumentKind.Remote.GetNativeDocumentKindRemote(CitizenRequests.PublicConstants.Module.TransferNotificationKind);
      var notification = CitizenRequests.OutgoingRequestLetters.As(_obj.CoverDocumentsGroup.OfficialDocuments.Where(d => Equals(d.DocumentKind, notificationKind)).FirstOrDefault());
      
      var request = CitizenRequests.Requests.As(_obj.DocumentsGroup.OfficialDocuments.FirstOrDefault());
      
      var resolution = _obj.ResolutionGroup.ActionItemExecutionTasks.Any() ?
        MainSolution.ActionItemExecutionTasks.As(_obj.ResolutionGroup.ActionItemExecutionTasks.FirstOrDefault()) :
        MainSolution.Module.CitizenRequests.PublicFunctions.Module.Remote.GetActualActionItemExecutionTask(_obj);;
      
      var errorText = CitizenRequests.PublicFunctions.Module.CheckCreateNotification(notification, request, resolution);
      if (!string.IsNullOrEmpty(errorText))
      {
        eventArgs.AddError(errorText);
        return;
      }
      
      var  notificationTransfer = CitizenRequests.PublicFunctions.Module.Remote.CreateOrUpdateTransferNotification(resolution, notification);
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
    public virtual bool CheckActualityLettersForExecution(GovernmentSolution.IActionItemExecutionTask resolution, Sungero.Domain.Client.ExecuteActionArgs eventArgs)
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
        return CreateTransferDocumentsForExecution(errorCoverLetter, errorNotification, eventArgs);
      else
        return true;
      
    }
  }
}