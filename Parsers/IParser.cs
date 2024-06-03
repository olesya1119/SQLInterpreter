using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.Parsers
{
    public interface IParser
    {
        string GetResult(Table table, string args);
    }
}
