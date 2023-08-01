using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.PrepareDraftActionItemAssignment;

namespace GD.MainSolution.Client
{
  partial class PrepareDraftActionItemAssignmentActions
  {

    public virtual void AddActionItem(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      var newTask = GD.MainSolution.PublicFunctions.PrepareDraftActionItemAssignment.Remote.CreateActionItem(_obj);
      
      if (newTask != null)
      {
        newTask.ShowModal();
        
        if (!newTask.State.IsInserted)
        {
          _obj.DraftActionItemGroup.ActionItemExecutionTasks.Add(newTask);
          _obj.Save();
        }
      }
    }

    public virtual bool CanAddActionItem(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return _obj.Status == Sungero.Workflow.Assignment.Status.InProcess && _obj.DraftActionItemGroup.ActionItemExecutionTasks.FirstOrDefault() == null;
    }

    public virtual void SendForExecute(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanSendForExecute(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void Explored(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanExplored(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

    public virtual void SendForReview(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanSendForReview(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }


}