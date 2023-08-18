using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.ActionItemExecutionTask;

namespace GD.MainSolution.Shared
{
  partial class ActionItemExecutionTaskFunctions
  {
    /// <summary>
    /// Создать список код вопроса, вопрос из обращения по guid.
    /// </summary>
    /// <param name="questionGuids">Список guid строк табличной части вопросов обращения.</param>
    /// <param name="request">Обращение.</param>
    /// <param name="isDetailedData">Детальные данные.</param>
    public override List<string> GetQuestionListByGuids(List<string> questionGuids, CitizenRequests.IRequest request, bool isDetailedData)
    {
      return base.GetQuestionListByGuids(questionGuids, request, isDetailedData);
    }
    /// <summary>
    /// Заполнить вопросы из пунктов поручения резолюции в проект поручения.
    /// </summary>
    public override void FillQuestionsFromRequestTransfer(List<string> excludeQuestionsGuids)
    {
      base.FillQuestionsFromRequestTransfer(excludeQuestionsGuids);
    }
    /// <summary>
    /// Заполнить вопросы из пунктов поручения резолюции в проект поручения.
    /// </summary>
    /// <param name="resolution">Резолюция.</param>
    public virtual void FillQuestionsFromResolution(MainSolution.IActionItemExecutionTask resolution)
    {
      // Заполнить ТЧ перенаправление из резолюции.
      var actionItemParts = resolution.ActionItemParts.Where(a => Equals(a.Assignee, _obj.AssignedBy));
      foreach (var row in actionItemParts.Cast<IActionItemExecutionTaskActionItemParts>())
      {
        if (row.QuestionRowGuidGD !=null)
        {
          var part = _obj.QuestionsForTransferGD.AddNew();
          part.Question = row.RequestQuestionGD;
          part.SubQuestion = row.RequestSubQuestionGD;
          part.QuestionRowGuid = row.QuestionRowGuidGD;
          part.ActionItemPart = row.ActionItemPart;
        }
      }
    }
    
    public static string GetActionItemExecutionSubjectGD(IActionItemExecutionTask task, CommonLibrary.LocalizedString beginningSubject)
    {
      var subject = GetActionItemExecutionSubject(task, beginningSubject);
      return subject.Substring(0, subject.Length > 250 ? 250 : subject.Length);
    }
  }
}