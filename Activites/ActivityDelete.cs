using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    internal class ActivityDelete : IActivity
    {
        public int Counter { get; set; }
        public void Do(Entry entry)
        {
            if (!entry.IsDeleted)
            {
                entry.IsDeleted = true;
                Counter++;
            }
        }
    }
}
