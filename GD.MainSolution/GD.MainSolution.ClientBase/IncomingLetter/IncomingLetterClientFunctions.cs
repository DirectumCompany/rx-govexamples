using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.IncomingLetter;

namespace GD.MainSolution.Client
{
  partial class IncomingLetterFunctions
  {
    /// <summary>
    /// Отобразить диалог для вывода дубликатов.
    /// </summary>
    public void ShowDialogIfHaveDuplicates(Sungero.Docflow.IDocumentKind documentKind,
                                           Sungero.Company.IBusinessUnit businessUnit,
                                           string correspondentNumber,
                                           DateTime? dated,
                                           Sungero.Parties.ICounterparty correspondent)
    {
      if (documentKind == null ||
          businessUnit == null ||
          string.IsNullOrEmpty(correspondentNumber) ||
          !dated.HasValue ||
          correspondent == null)
        return;
      
      var duplicates = Functions.IncomingLetter.Remote.GetDuplicates(_obj, documentKind, businessUnit, correspondentNumber, dated, correspondent);
      if (duplicates.Any())
      {
        var dialog = Dialogs.CreateTaskDialog(MainSolution.IncomingLetters.Resources.ShowDuplicates, "", MessageType.Question, MainSolution.IncomingLetters.Resources.Attention);
        dialog.Buttons.AddYes();
        dialog.Buttons.AddCancel();
        if (dialog.Show() == DialogButtons.Yes)          
          duplicates.ShowModal();        
      }
    }
  }
}