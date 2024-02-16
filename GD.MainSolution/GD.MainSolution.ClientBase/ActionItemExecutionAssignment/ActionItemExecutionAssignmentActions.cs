using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionAssignment;

namespace GD.MainSolution.Client
{
  partial class ActionItemExecutionAssignmentActions
  {
    public override void CreateCoverLetterGD(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.CreateCoverLetterGD(e);
      var document = _obj.DocumentsGroup.OfficialDocuments.FirstOrDefault();
      if (_obj.ResultGroup.OfficialDocuments.Any() && IncomingDocumentBases.Is(document) && IncomingDocumentBases.As(document).ActionItemGD == null)
      {
        Functions.ActionItemExecutionAssignment.Remote.FillActionItemInIncomingDocumentIgnoreRights(_obj, IncomingDocumentBases.As(document));
      }
    }

    public override bool CanCreateCoverLetterGD(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateCoverLetterGD(e);
    }

    public override void CreateReplyLetter(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.CreateReplyLetter(e);
      var document = _obj.DocumentsGroup.OfficialDocuments.FirstOrDefault();
      if (_obj.ResultGroup.OfficialDocuments.Any() && IncomingDocumentBases.Is(document) && IncomingDocumentBases.As(document).ActionItemGD == null)
      {
        Functions.ActionItemExecutionAssignment.Remote.FillActionItemInIncomingDocumentIgnoreRights(_obj, IncomingDocumentBases.As(document));
      }
    }

    public override bool CanCreateReplyLetter(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateReplyLetter(e);
    }

    public override void CreateChildActionItem(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      e.CloseFormAfterAction = true;
      // base.CreateChildActionItem(e);
      Logger.DebugFormat("ActionItemExecutionAssignment (ID={0}). Start CreateChildActionItem.", _obj.Id);
      var subTask = Functions.ActionItemExecutionTask.Remote.CreateActionItemExecutionFromExecution(ActionItemExecutionTasks.As(_obj.Task), _obj);
      subTask.ShowModal();
      Logger.DebugFormat("ActionItemExecutionAssignment (ID={0}). End CreateChildActionItem.", _obj.Id);
      e.CloseFormAfterAction = true;
    }

    public override bool CanCreateChildActionItem(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanCreateChildActionItem(e);
    }

  }

}