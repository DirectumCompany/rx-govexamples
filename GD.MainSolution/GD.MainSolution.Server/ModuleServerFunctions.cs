using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace GD.MainSolution.Server
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Получить документы, связанные типом связи кроме "Приложение", если текущий документ - источник и документы, связанные типом связи "Основание", "Отмена" и "Прочие", если текущий документ - связанный.
    /// </summary>
    /// <param name="document">Документ.</param>
    /// <param name="isSource">Документ - источник связи?.</param>
    /// <returns>Связанны документы, удовлетворяющие заданным параметрам.</returns>
    [Remote]
    public IQueryable<Sungero.Content.IElectronicDocument> GetRelatedDocuments(Sungero.Docflow.IOfficialDocument document, bool isSource)
    {
      if (isSource)
        return Sungero.Content.DocumentRelations.GetAll()
          .Where(x => Equals(x.Source, document) && !Equals(x.RelationType.Name, Sungero.Docflow.PublicConstants.Module.AddendumRelationName)).Select(x => x.Target);
      else
        return Sungero.Content.DocumentRelations.GetAll()
          .Where(x => Equals(x.Target, document) && (Equals(x.RelationType.Name, Sungero.Docflow.PublicConstants.Module.BasisRelationName) ||
                                                     Equals(x.RelationType.Name, Sungero.Docflow.PublicConstants.Module.CancelRelationName) ||
                                                     Equals(x.RelationType.Name, Sungero.Docflow.PublicConstants.Module.SimpleRelationName))).Select(x => x.Source);
    }
  }
}
