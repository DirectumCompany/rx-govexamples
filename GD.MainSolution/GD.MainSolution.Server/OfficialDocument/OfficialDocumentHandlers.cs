using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.OfficialDocument;

namespace GD.MainSolution
{
  partial class OfficialDocumentServerHandlers
  {

    public override void AfterSave(Sungero.Domain.AfterSaveEventArgs e)
    {
      base.AfterSave(e);
      
      var needUpdateActionItems = false;
      if (e.Params.TryGetValue(Constants.Docflow.OfficialDocument.NeedUpdateActionItemsParamName, out needUpdateActionItems))
        e.Params.Remove(Constants.Docflow.OfficialDocument.NeedUpdateActionItemsParamName);
      
      if (needUpdateActionItems)
      {
        var asyncHandler = Module.RecordManagement.AsyncHandlers.UpdateDocumentDataInActionItemGD.Create();
        asyncHandler.DocumentId = _obj.Id;
        asyncHandler.ExecuteAsync();
      }
    }

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      base.BeforeSave(e);
      
      if (!_obj.State.IsInserted)
      {
        var props = _obj.State.Properties;
        var needUpdateActionItems = props.RegistrationNumber.IsChanged ||
          props.RegistrationDate.IsChanged ||
          props.DocumentRegister.IsChanged ||
          props.DocumentKind.IsChanged;
        e.Params.AddOrUpdate(Constants.Docflow.OfficialDocument.NeedUpdateActionItemsParamName, needUpdateActionItems);
      }
    }

  }
}