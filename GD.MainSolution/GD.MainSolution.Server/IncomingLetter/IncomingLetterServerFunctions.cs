using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.IncomingLetter;

namespace GD.MainSolution.Server
{
  partial class IncomingLetterFunctions
  {
    /// <summary>
    /// Получить дубли письма.
    /// </summary>
    /// <param name="letter">Письмо для проверки.</param>
    /// <param name="documentKind">Вид  документа.</param>
    /// <param name="businessUnit">НОР письма.</param>
    /// <param name="inNumber">Номер входящего.</param>
    /// <param name="dated">Дата письма.</param>
    /// <param name="correspondent">Корреспондент.</param>
    /// <returns>Письма, дублирующие текущее.</returns>
    [Remote(IsPure = true)]
    public static IQueryable<IIncomingLetter> GetDuplicates(IIncomingLetter letter,
                                                            Sungero.Docflow.IDocumentKind documentKind,
                                                            Sungero.Company.IBusinessUnit businessUnit,
                                                            string inNumber,
                                                            DateTime? dated,
                                                            Sungero.Parties.ICounterparty correspondent)
    {
      return IncomingLetters.GetAll()
        .Where(l => documentKind != null && Equals(documentKind, l.DocumentKind))
        .Where(l => dated.HasValue && dated == l.Dated)
        .Where(l => businessUnit != null && Equals(businessUnit, l.BusinessUnit))
        .Where(l => !string.IsNullOrWhiteSpace(inNumber) && inNumber == l.InNumber)
        .Where(l => correspondent != null && Equals(correspondent, l.Correspondent))
        .Where(l => !Equals(letter, l));
    }
  }
}