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
    /// Проверить, есть ли подходящие настройки регистрации для определения исполнителя на этапе регистрации.
    /// </summary>
    /// <returns>true, если есть. Иначе - false.</returns>
    [Public]
    public bool ExistsRegistrationSetting()
    {
      if (_obj.DocumentKind == null)
        return false;
      
      return Sungero.Docflow.RegistrationSettings.GetAll()
        .Where(s => s.SettingType == Sungero.Docflow.RegistrationSetting.SettingType.Registration &&
               s.Status == Sungero.CoreEntities.DatabookEntry.Status.Active && Equals(s.DocumentFlow, _obj.DocumentKind.DocumentFlow) &&
               (!s.DocumentKinds.Any() || s.DocumentKinds.Any(k => Equals(k.DocumentKind, _obj.DocumentKind))) &&
               (!s.BusinessUnits.Any() || s.BusinessUnits.Any(u => Equals(u.BusinessUnit, _obj.BusinessUnit))) &&
               (!s.Departments.Any() || s.Departments.Any(d => Equals(d.Department, _obj.Department))))
        .Select(s => s.DocumentRegister.RegistrationGroup)
        .Any(s => !s.Departments.Any() || s.Departments.Any(d => Equals(d.Department, _obj.Department)));
    }
    
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