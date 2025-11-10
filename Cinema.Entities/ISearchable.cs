using System.Collections.Generic;

namespace Cinema.Entities;

public interface ISearchable<TCriteria, TResult>
{
    List<TResult> Search(TCriteria criteria);
}
