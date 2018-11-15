using System;
using System.Collections.Generic;
using System.Text;

namespace Monitoring.Nest.App.Model
{
    class WhereList
    {
        public List<Where> Wheres { get; set; }
    }

    public class Where
    {
        public string Name { get; set; }
        public string WhereId { get; set; }
    }
}
