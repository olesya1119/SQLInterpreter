using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    public class ParserDelete
    {
        private string command;
        private Table table;
        private IActivity activity;


        public ParserDelete(string command, Table table)
        {
            this.command = command;
            this.table = table;
            activity = new ActivityDelete();
        }

        public string GetResult()
        {
            ParserWhere parserWhere = new ParserWhere(table, command);

            List<Entry> entries = table.RunForArray(activity, parserWhere.GetResult());

            return "Изменено " + entries.Count + "строк.\n"; //Результирующая строка

        }
    }
}
