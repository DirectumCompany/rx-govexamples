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
      // ������ �� ����.
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
                               // ������������.
                               ApprovalReviewAssignments.Is(q) && ApprovalReviewAssignments.As(q).CollapsedStagesTypesRe.Any(s => stageTypes.Contains(s.StageType.Value)) ||
                               // ����������.
                               ApprovalSigningAssignments.Is(q) && ApprovalSigningAssignments.As(q).CollapsedStagesTypesSig.Any(s => stageTypes.Contains(s.StageType.Value)) ||
                               // �������� ���������.
                               (ApprovalExecutionAssignments.Is(q) && ApprovalExecutionAssignments.As(q).CollapsedStagesTypesExe.Any(s => stageTypes.Contains(s.StageType.Value)) ||
                                showExecution && ReviewResolutionAssignments.Is(q)) ||
                               // ���������� ������� ���������.
                               showExecution && PreparingDraftResolutionAssignments.Is(q) ||
                               // �����������.
                               ApprovalRegistrationAssignments.Is(q) && ApprovalRegistrationAssignments.As(q).CollapsedStagesTypesReg.Any(s => stageTypes.Contains(s.StageType.Value)) ||
                               // ������.
                               ApprovalPrintingAssignments.Is(q) && ApprovalPrintingAssignments.As(q).CollapsedStagesTypesPr.Any(s => stageTypes.Contains(s.StageType.Value)) ||
                               // �������� �����������.
                               ApprovalSendingAssignments.Is(q) && ApprovalSendingAssignments.As(q).CollapsedStagesTypesSen.Any(s => stageTypes.Contains(s.StageType.Value)) ||
                               // �������� ��������.
                               showCheckReturn && ApprovalCheckReturnAssignments.Is(q) ||
                               // ������ �������.
                               showOther && (ApprovalSimpleAssignments.Is(q) || ApprovalCheckingAssignments.Is(q) || ReviewReworkAssignments.Is(q)) ||
                               // ������� �� ���������� ������� ���������.
                               PrepareDraftActionItemAssignments.Is(q));
      
      // ������ ������������� ��� �������.
      if (_filter == null)
        return Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(result);
      
      // ������� �� �������, ��������� � �������.
      result = Sungero.RecordManagement.PublicFunctions.Module.ApplyCommonSubfolderFilters(result, _filter.InProcess,
                                                                                   _filter.Last30Days, _filter.Last90Days, _filter.Last180Days, false);
      return result;
    }
  }

  partial class ShellHandlers
  {
  }
}