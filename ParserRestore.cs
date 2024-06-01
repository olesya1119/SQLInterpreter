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
        IActivity activity;


        public ParserRestore(string command, Table table)
        {
            this.command = command;
            this.table = table;
            activity = new ActivityRestore();
        }


        public string GetResult()
        {
            ParserWhere parserWhere = new ParserWhere(table, command);
            LogicEntries logicEntries = parserWhere.GetResult();
    
            return "Изменено " + table.RunForArray(activity, logicEntries) + "строк.\n"; //Результирующая строка

        }
 
    }
}
