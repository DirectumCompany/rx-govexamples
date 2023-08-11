using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace GD.MainSolution.Module.Shell.Server
{
  partial class OnDocumentProcessingFolderHandlers
  {

    public override IQueryable<Sungero.Workflow.IAssignmentBase> OnDocumentProcessingDataQuery(IQueryable<Sungero.Workflow.IAssignmentBase> query)
    {
      // Фильтр по типу.
      var typeFilterEnabled = _filter != null && (_filter.ProcessResolution ||
                                                  _filter.ConfirmSigning ||
                                                  _filter.SendActionItem ||
                                                  _filter.Send ||
                                                  _filter.CheckReturn ||
                                                  _filter.Other);
      
      var stageTypes = new List<Sungero.Core.Enumeration>();
      if (!typeFilterEnabled || _filter.ProcessResolution)
        stageTypes.Add(Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.ReviewingResult);
      if (!typeFilterEnabled || _filter.ConfirmSigning)
        stageTypes.Add(Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.ConfirmSign);
      if (!typeFilterEnabled || _filter.SendActionItem)
        stageTypes.Add(Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.Execution);
      if (!typeFilterEnabled || _filter.Send)
        stageTypes.Add(Sungero.Docflow.ApprovalReviewAssignmentCollapsedStagesTypesRe.StageType.Sending);
      
      var showExecution = !typeFilterEnabled || _filter.SendActionItem;
      var showCheckReturn = !typeFilterEnabled || _filter.CheckReturn;
      var showOther = !typeFilterEnabled || _filter.Other;
      
      var result = query.Where(q =>
                               // Рассмотрение.
                               ApprovalReviewAssignments.Is(q) && ApprovalReviewAssignments.As(q).CollapsedStagesTypesRe.Any(s => stageTypes.Contains(s.StageType.Value)) ||
                               // Подписание.
                               ApprovalSigningAssignments.Is(q) && ApprovalSigningAssignments.As(q).CollapsedStagesTypesSig.Any(s => stageTypes.Contains(s.StageType.Value)) ||
                               // Создание поручений.
                               (ApprovalExecutionAssignments.Is(q) && ApprovalExecutionAssignments.As(q).CollapsedStagesTypesExe.Any(s => stageTypes.Contains(s.StageType.Value)) ||
                                showExecution && ReviewResolutionAssignments.Is(q)) ||
                               // Подготовка проекта резолюции.
                               showExecution && PreparingDraftResolutionAssignments.Is(q) ||
                               // Регистрация.
                               ApprovalRegistrationAssignments.Is(q) && ApprovalRegistrationAssignments.As(q).CollapsedStagesTypesReg.Any(s => stageTypes.Contains(s.StageType.Value)) ||
                               // Печать.
                               ApprovalPrintingAssignments.Is(q) && ApprovalPrintingAssignments.As(q).CollapsedStagesTypesPr.Any(s => stageTypes.Contains(s.StageType.Value)) ||
                               // Отправка контрагенту.
                               ApprovalSendingAssignments.Is(q) && ApprovalSendingAssignments.As(q).CollapsedStagesTypesSen.Any(s => stageTypes.Contains(s.StageType.Value)) ||
                               // Контроль возврата.
                               showCheckReturn && ApprovalCheckReturnAssignments.Is(q) ||
                               // Прочие задания.
                               showOther && (ApprovalSimpleAssignments.Is(q) || ApprovalCheckingAssignments.Is(q) || ReviewReworkAssignments.Is(q)) ||
                               // Задания на подготовку проекта поручения.
                               PrepareDraftActionItemAssignments.Is(q));
      
      // Запрос непрочитанных без фильтра.
      if (_filter == null)
        return Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(result);
      
      // Фильтры по статусу, замещению и периоду.
      result = Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(result, _filter.InProcess,
                                                                                   _filter.Last30Days, _filter.Last90Days, _filter.Last180Days, false);
      return result;
    }
  }

  partial class ShellHandlers
  {
  }
}