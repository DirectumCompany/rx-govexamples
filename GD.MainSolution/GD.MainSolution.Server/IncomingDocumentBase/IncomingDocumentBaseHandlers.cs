using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using GD.MainSolution.IncomingDocumentBase;

namespace GD.MainSolution
{
  partial class IncomingDocumentBaseFilteringServerHandler<T>
  {

    public override IQueryable<T> Filtering(IQueryable<T> query, Sungero.Domain.FilteringEventArgs e)
    {
      query = base.Filtering(query, e);
      // Фильтрация по году.
      if (_filter.CurrentYear)
      {
        query = query.Where(x => x.DocumentDate.Value.Year == Calendar.Today.Year);
      }
      else if (_filter.PreviousYear)
      {
        query = query.Where(x => x.DocumentDate.Value.Year == Calendar.Today.Year - 1);
      }
      else if (_filter.PeriodYear)
      {
        var yearBegin = _filter.YearFrom != null ? _filter.YearFrom.Value.Year : Calendar.SqlMinValue.Year;
        var yearEnd = _filter.YearTo != null ? _filter.YearTo.Value.Year : Calendar.SqlMaxValue.Year;
        query = query.Where(x => x.DocumentDate.Value.Year >= yearBegin && x.DocumentDate.Value.Year <= yearEnd);
      }
      return query;
    }
  }

}