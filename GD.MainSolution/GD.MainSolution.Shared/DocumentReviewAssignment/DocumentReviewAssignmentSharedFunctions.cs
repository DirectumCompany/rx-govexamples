using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.DocumentReviewAssignment;

namespace GD.MainSolution.Shared
{
  partial class DocumentReviewAssignmentFunctions
  {
    /// <summary>
    /// Проверить наличие документа на рассмотрение в задании и наличие хоть каких-то прав на него.
    /// </summary>
    /// <returns>True, если с документом можно работать.</returns>
    [Public]
    public override bool HasDocumentAndCanRead()
    {
      // Для задачи на исполнение поручения убираем данное требование т.к. документа может не быть.
      var isActionItemExecutionTask = ActionItemExecutionTasks.Is(_obj.Task);
      return isActionItemExecutionTask || base.HasDocumentAndCanRead();
    }
  }
}