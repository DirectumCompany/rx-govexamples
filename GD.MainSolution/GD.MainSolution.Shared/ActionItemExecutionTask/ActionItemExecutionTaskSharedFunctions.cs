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
    /// Проверить вопросы по обращению на рассмотрение другими адресатами или на исполнение.
    /// </summary>
    /// <param name="questionGuids">Список guid вопросов по обращению.</param>
    /// <param name="args">Аргумент события.</param>
    public override void CheckQuestionsforReviewOtherAddressee(List<string> questionGuids, Sungero.Core.IValidationArgs args)
    {
      base.CheckQuestionsforReviewOtherAddressee(questionGuids, args);
    }
    
    /// <summary>
    /// Заполнить вопросы из пунктов поручения резолюции в проект поручения.
    /// </summary>
    public override void FillQuestionsFromRequestTransfer()
    {
      base.FillQuestionsFromRequestTransfer();
    }
    
    /// <summary>
    /// Заполнить вопросы из пунктов поручения резолюции в проект поручения.
    /// </summary>
    /// <param name="resolution">Резолюция.</param>
    public virtual void FillQuestionsFromResolution(MainSolution.IActionItemExecutionTask resolution)
    {
      
      // Заполнить ТЧ на исполнение из резолюции.
      var actionItemParts = resolution.ActionItemParts.Where(a => Equals(a.Assignee, _obj.AssignedBy));
      foreach (var row in actionItemParts.Cast<IActionItemExecutionTaskActionItemParts>())
      {
        var part = _obj.ActionItemParts.AddNew() as IActionItemExecutionTaskActionItemParts;
        part.RequestQuestionGD = row.RequestQuestionGD;
        part.RequestSubQuestionGD = row.RequestSubQuestionGD;
        part.QuestionRowGuidGD = row.QuestionRowGuidGD;
        part.ActionItemPart = row.ActionItemPart;
        part.Deadline = row.Deadline;
        
        // Добавить соисполнителей.
        if (!string.IsNullOrEmpty(row.CoAssignees))
        {
          var coAssignees = GovernmentSolution.PublicFunctions.ActionItemExecutionTask.GetPartCoAssignees(resolution, row.PartGuid);
          Sungero.RecordManagement.PublicFunctions.ActionItemExecutionTask.AddPartsCoAssignees(_obj, part, coAssignees);
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