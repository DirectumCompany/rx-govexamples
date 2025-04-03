using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.DocumentFlowTask;

namespace GD.MainSolution.Server
{
  partial class DocumentFlowTaskFunctions
  {
    /// <summary>
    /// Проверить, есть ли в задаче на согласование по процессу блок обработки и регистрации.
    /// </summary>
    /// <returns>true, если содержится. Иначе - false.</returns>
    public bool ContainsRegisterBlock()
    {
      return Sungero.DocflowApproval.Blocks.DocumentProcessingBlocks.GetAll(_obj.Scheme).Any();
    }

  }
}