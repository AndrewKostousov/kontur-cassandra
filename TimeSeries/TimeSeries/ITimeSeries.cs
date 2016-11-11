using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CassandraTimeSeries
{
    public interface ITimeSeries
    {
        void Write(Event ev);
        IEnumerable<Event> ReadRange(DateTimeOffset from, DateTimeOffset to);
    }
}
