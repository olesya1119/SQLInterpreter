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
        private ActivityDelete _activity;

        public ParserDelete()
        { 
            _activity = new ActivityDelete();
        }

        public string GetResult(Table table, string args)
        {
            _activity = new ActivityDelete();
            ParserWhere parserWhere = new ParserWhere(table, args);
            table.RunForArray(_activity, parserWhere.GetResult());

            return "Изменено " +_activity.Counter + " строк."; //Результирующая строка
        }
    }
}
