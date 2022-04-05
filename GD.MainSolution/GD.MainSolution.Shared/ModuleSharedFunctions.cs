using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace GD.MainSolution.Shared
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Добавить в задачу все документы по связям кроме "Приложение" и документы с типом связи "Основание", "Отмена" и "Прочие".
    /// <param name="groupAttachments">Группа вложения задачи.</param>
    /// <param name="document">Документ.</param>
    /// </summary>
    public void AddRelationToAddendum(Sungero.Workflow.Interfaces.IWorkflowEntityAttachmentGroup groupAttachments, Sungero.Docflow.IOfficialDocument document)
    {
      if (document == null)
        return;
      
      var docsSource = Functions.Module.Remote.GetRelatedDocuments(document, true);
      foreach (var relatedDoc in docsSource)
        if (relatedDoc != null && !groupAttachments.All.Contains(relatedDoc))
          groupAttachments.All.Add(relatedDoc);
      
      Sungero.Docflow.PublicFunctions.OfficialDocument.AddRelatedDocumentsToAttachmentGroup(document, groupAttachments);
      
      var docsTarget = Functions.Module.Remote.GetRelatedDocuments(document, false);
      foreach (var relatedDoc in docsTarget)
        if (relatedDoc != null && !groupAttachments.All.Contains(relatedDoc))
          groupAttachments.All.Add(relatedDoc);
    }
  }
}