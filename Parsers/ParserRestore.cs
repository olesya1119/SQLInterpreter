using SQLInterpreter.Parsers;
using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    public class ParserRestore : IParser
    {
        private ActivityRestore _activity;

        /// <summary>
        /// Создание экземпляра класса 
        /// </summary>
        public ParserRestore()
        {
            _activity = new ActivityRestore();
        }

        /// <summary>
        /// Получить результат выполнения команды RESTORE
        /// </summary>
        /// <param name="table">Таблица в котороый</param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string GetResult(Table table, string args)
        {
            _activity = new ActivityRestore();
            ParserWhere parserWhere = new ParserWhere(table, args);
            LogicEntries logicEntries = parserWhere.GetResult();
            table.RunForArray(_activity, logicEntries);
            return "Изменено " + _activity.Counter + " строк."; //Результирующая строка
        }
    }
}
