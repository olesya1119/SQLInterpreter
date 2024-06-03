using SQLInterpreter.Parsers;
using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    public class ParserDelete : IParser
    {
        private IActivity _activity;

        public ParserDelete()
        { 
            _activity = new ActivityDelete();
        }

        public string GetResult(Table table, string args)
        {
 
            ParserWhere parserWhere = new ParserWhere(table, args);
            List<Entry> entries = table.RunForArray(_activity, parserWhere.GetResult());

            return "Изменено " + entries.Count + " строк."; //Результирующая строка
        }
    }
}
