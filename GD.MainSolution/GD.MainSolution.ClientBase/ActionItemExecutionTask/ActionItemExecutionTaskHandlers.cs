using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionTask;

namespace GD.MainSolution
{
  partial class ActionItemExecutionTaskClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      base.Refresh(e);
      _obj.State.Attachments.CoverDocumentsGroup.IsVisible = _obj.CoverDocumentsGroup.All.Any();      
    }
  }
}