﻿using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    internal class ActivityDelete : IActivity
    {
        public void Do(Entry entry)
        {
            entry.IsDeleted = true;
        }
    }
}
