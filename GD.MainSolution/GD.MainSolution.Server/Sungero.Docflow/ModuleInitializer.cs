using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace GD.MainSolution.Module.Docflow.Server
{
  public partial class ModuleInitializer
  {
    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      base.Initializing(e);
      UpdateAccessRightsRegistrationManager();
    }
    
    /// <summary>
    // Запрет полного доступа на виды документов, журналы документов, настройки регистрации роли "Ответственные за настройку регистрации"
    /// </summary>
    private static void UpdateAccessRightsRegistrationManager()
    {
      InitializationLogger.Debug("Init: Update rights on registration settings, document registers, document kinds to registration managers.");
      var registrationManagers = Roles.GetAll().FirstOrDefault(n => n.Sid == Sungero.Docflow.Constants.Module.RoleGuid.RegistrationManagersRole);
      if (registrationManagers == null)
        return;
      Sungero.Docflow.DocumentKinds.AccessRights.RevokeAll(registrationManagers);
      Sungero.Docflow.DocumentKinds.AccessRights.Save();
      Sungero.Docflow.DocumentRegisters.AccessRights.RevokeAll(registrationManagers);
      Sungero.Docflow.DocumentRegisters.AccessRights.Grant(registrationManagers, DefaultAccessRightsTypes.Change);
      Sungero.Docflow.DocumentRegisters.AccessRights.Save();
      Sungero.Docflow.RegistrationSettings.AccessRights.RevokeAll(registrationManagers);
      Sungero.Docflow.RegistrationSettings.AccessRights.Save();
    }
  }
}
