using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.DocumentFlowTask;

namespace GD.MainSolution
{
  partial class DocumentFlowTaskServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      base.Created(e);
      _obj.IsParallelGD = false;
    }
  }

}