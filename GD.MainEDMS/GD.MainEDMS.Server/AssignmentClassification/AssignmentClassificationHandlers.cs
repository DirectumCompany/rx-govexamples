using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainEDMS.AssignmentClassification;

namespace GD.MainEDMS
{
  partial class AssignmentClassificationServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      _obj.AccessRights.Grant(Sungero.Company.Employees.Current, DefaultAccessRightsTypes.FullAccess);
    }
  }

  partial class AssignmentClassificationFilteringServerHandler<T>
  {

    public override IQueryable<T> Filtering(IQueryable<T> query, Sungero.Domain.FilteringEventArgs e)
    {
      if (_filter == null)
        return query;
      
      if (_filter.Active || _filter.Closed)
        query = query.Where(r => _filter.Active && r.Status == Status.Active ||
                            _filter.Closed && r.Status == Status.Closed);
      return query;
    }
  }

}