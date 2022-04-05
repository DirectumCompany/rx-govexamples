using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionTask;

namespace GD.MainSolution
{
  partial class ActionItemExecutionTaskSharedHandlers
  {

    public override void DocumentsGroupAdded(Sungero.Workflow.Interfaces.AttachmentAddedEventArgs e)
    {
      base.DocumentsGroupAdded(e);
      var document = Sungero.Docflow.OfficialDocuments.As(e.Attachment);
      _obj.DocumentGD = document;
      _obj.RegNumberGD = document.RegistrationNumber;
      _obj.RegDateGD = document.RegistrationDate;
      if (IncomingDocumentBases.Is(document))
        Functions.Module.AddRelationToAddendum(_obj.OtherGroup, document);
    }

    public override void DocumentsGroupDeleted(Sungero.Workflow.Interfaces.AttachmentDeletedEventArgs e)
    {
      base.DocumentsGroupDeleted(e);
      _obj.DocumentGD = null;
      _obj.RegNumberGD = null;
      _obj.RegDateGD = null;
      foreach (var relatedDoc in _obj.OtherGroup.All)
        _obj.OtherGroup.All.Remove(relatedDoc);
    }
  }
}