using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace GD.MainSolution.Module.RecordManagement.Shared
{
  partial class ModuleFunctions
  {
    /// <summary>
    /// Синхронизировать вложения во вновь созданное поручение.
    /// </summary>
    /// <param name="primaryDocument">Основной документ.</param>
    /// <param name="addenda">Вложения из группы "Приложения".</param>
    /// <param name="addedAddendaIds">Список ИД добавленных приложений.</param>
    /// <param name="removedAddendaIds">Список ИД удалённых приложений.</param>
    /// <param name="otherAttachments">Вложения из группы "Дополнительно".</param>
    /// <param name="actionItem">Поручение, в которое будут добавлены вложения.</param>
    [Public]
    public override void SynchronizeAttachmentsToActionItem(Sungero.Docflow.IOfficialDocument primaryDocument,
                                                            List<Sungero.Content.IElectronicDocument> addenda,
                                                            List<int> addedAddendaIds,
                                                            List<int> removedAddendaIds,
                                                            List<Sungero.Domain.Shared.IEntity> otherAttachments,
                                                            Sungero.RecordManagement.IActionItemExecutionTask actionItem)
    {
      base.SynchronizeAttachmentsToActionItem(primaryDocument, addenda, addedAddendaIds, removedAddendaIds, otherAttachments, actionItem);
      
      if (primaryDocument != null && actionItem != null)
      {
        var incomingLetter = IncomingLetters.As(primaryDocument);
        
        if (incomingLetter != null)
        {
          var docsSourceAndDocsTarget = new List<Sungero.Content.IElectronicDocument>();
          docsSourceAndDocsTarget.AddRange(GD.MainSolution.Functions.Module.Remote.GetRelatedDocuments(primaryDocument, true).ToList());
          docsSourceAndDocsTarget.AddRange(GD.MainSolution.Functions.Module.Remote.GetRelatedDocuments(primaryDocument, false).ToList());
          
          foreach (var document in docsSourceAndDocsTarget)
          {
            if (!actionItem.OtherGroup.All.Contains(document) && !actionItem.AddendaGroup.All.Contains(document))
              actionItem.OtherGroup.All.Add(document);
          }
          
        }
      }
    }
  }
}