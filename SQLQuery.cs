using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    /// <summary>
    /// Хранит параметры SQL запроса
    /// </summary>
    internal class SQLQuery
    {
        public ICommand Command { get; set; }
        public string TableName {  get; set; }
        public string Arguments { get; set; }

        public SQLQuery() { 
            Command.TableName = TableName;
            Command.Arguments = Arguments;
            Command.Execute();
        }
    }
}
