using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.OfficialDocument;

namespace GD.MainSolution.Server
{
  partial class OfficialDocumentFunctions
  {
    /// <summary>
    /// Отметить документы устаревшими.
    /// </summary>
    /// <param name="documents">Список документов.</param>
    [Public, Remote]
    public static void MarkDocumentsAsObsolete(List<Sungero.Docflow.IOfficialDocument> documents)
    {
      foreach (var document in documents)
      {
        AccessRights.AllowRead(
          () =>
          {
            document.LifeCycleState = Sungero.Docflow.OfficialDocument.LifeCycleState.Obsolete;
            document.Save();
          });
      }
    }
  }
}