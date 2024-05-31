using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    public class ParserRestore
    {
        private string command;
        private Table table;


        public ParserRestore(string command, Table table)
        {
            this.command = command;
            this.table = table;
        }


        public string GetResult()
        {
            ParserWhere parserWhere = new ParserWhere(table, command);
            List<Entry> entries = parserWhere.GetResult();
    
            return "Изменено " + table.Restore(entries) + "строк.\n"; //Результирующая строка

        }
 
    }
}
