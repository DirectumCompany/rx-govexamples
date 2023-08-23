using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionAssignment;

namespace GD.MainSolution.Server
{
  partial class ActionItemExecutionAssignmentFunctions
  {

    /// <summary>
    /// ��������� ��������� �� �������� ���������, ��������� ����� �������.
    /// </summary>
    /// <param name="document">�������� ��������.</param>
    [Remote]
    public virtual void FillActionItemInIncomingDocumentIgnoreRights(IIncomingDocumentBase document)
    {
      AccessRights.AllowRead(
        () =>
        {
          document.ActionItemGD = _obj;
          document.Save();
        });
    }
  }
}