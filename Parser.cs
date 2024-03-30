using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    /// <summary>
    /// Парсер первого уровня
    /// </summary>

    delegate object[] CommandParser(string args);

    public class Parser
    {
        public void Parse(string input) { }
    }
}
