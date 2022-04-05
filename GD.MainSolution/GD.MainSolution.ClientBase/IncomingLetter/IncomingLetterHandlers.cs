using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.IncomingLetter;

namespace GD.MainSolution
{
  partial class IncomingLetterClientHandlers
  {

    public override void BusinessUnitValueInput(Sungero.Docflow.Client.OfficialDocumentBusinessUnitValueInputEventArgs e)
    {
      base.BusinessUnitValueInput(e);
      Functions.IncomingLetter.ShowDialogIfHaveDuplicates(_obj,
                                                          _obj.DocumentKind,
                                                          e.NewValue,
                                                          _obj.InNumber,
                                                          _obj.Dated,
                                                          _obj.Correspondent);
    }

    public override void DatedValueInput(Sungero.Presentation.DateTimeValueInputEventArgs e)
    {
      base.DatedValueInput(e);
      if (e.NewValue != null && e.NewValue >= Calendar.SqlMinValue)
        Functions.IncomingLetter.ShowDialogIfHaveDuplicates(_obj,
                                                            _obj.DocumentKind,
                                                            _obj.BusinessUnit,
                                                            _obj.InNumber,
                                                            e.NewValue,
                                                            _obj.Correspondent);
    }

    public override void InNumberValueInput(Sungero.Presentation.StringValueInputEventArgs e)
    {
      base.InNumberValueInput(e);
      Functions.IncomingLetter.ShowDialogIfHaveDuplicates(_obj,
                                                          _obj.DocumentKind,
                                                          _obj.BusinessUnit,
                                                          e.NewValue,
                                                          _obj.Dated,
                                                          _obj.Correspondent);
    }

    public override void CorrespondentValueInput(Sungero.Docflow.Client.IncomingDocumentBaseCorrespondentValueInputEventArgs e)
    {
      base.CorrespondentValueInput(e);
      Functions.IncomingLetter.ShowDialogIfHaveDuplicates(_obj,
                                                          _obj.DocumentKind,
                                                          _obj.BusinessUnit,
                                                          _obj.InNumber,
                                                          _obj.Dated,
                                                          e.NewValue);
    }

    public override void DocumentKindValueInput(Sungero.Docflow.Client.OfficialDocumentDocumentKindValueInputEventArgs e)
    {
      base.DocumentKindValueInput(e);
      Functions.IncomingLetter.ShowDialogIfHaveDuplicates(_obj,
                                                          e.NewValue,
                                                          _obj.BusinessUnit,
                                                          _obj.InNumber,
                                                          _obj.Dated,
                                                          _obj.Correspondent);
    }

  }
}