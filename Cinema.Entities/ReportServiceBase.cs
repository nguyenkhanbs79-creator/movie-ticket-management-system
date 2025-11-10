using System;
using System.Collections.Generic;

namespace Cinema.Entities;

public abstract class ReportServiceBase
{
    protected void ValidateRange(DateTime from, DateTime to)
    {
        if (from > to)
        {
            throw new ArgumentException("The 'from' date must be earlier than or equal to the 'to' date.");
        }
    }

    protected abstract IEnumerable<T> Query<T>(DateTime from, DateTime to);
    protected abstract IEnumerable<object> Query(DateTime from, DateTime to);
}
