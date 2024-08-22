using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace GD.MainEDMS.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      
      var allUsers = Roles.AllUsers;
      
      // Справочники.
      GrantRightsForAllUsers(allUsers);
      
      // Выдача прав роли "Делопроизводители".
      GrantRightsToClerksRole();
      
    }
    
    /// <summary>
    /// Выдать права всем пользователям.
    /// </summary>
    /// <param name="allUsers">Группа "Все пользователи".</param>
    public static void GrantRightsForAllUsers(IRole allUsers)
    {
      InitializationLogger.Debug("Init: Grant rights on databooks to all users.");
      
      // Классфикация поручений
      AssignmentClassifications.AccessRights.Grant(allUsers, DefaultAccessRightsTypes.Read);
      AssignmentClassifications.AccessRights.Save();
    }
    
    /// <summary>
    /// Назначить права роли "Делопроизводители".
    /// </summary>
    public static void GrantRightsToClerksRole()
    {
      InitializationLogger.Debug("Init: Grant rights on documents and databooks to clerks");
      
      var clerks = Sungero.Docflow.PublicFunctions.DocumentRegister.Remote.GetClerks();
      if (clerks == null)
        return;
      
      // Классфикация поручений
      AssignmentClassifications.AccessRights.Grant(clerks, DefaultAccessRightsTypes.Change);
      AssignmentClassifications.AccessRights.Save();
    }
  }
}
