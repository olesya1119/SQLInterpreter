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


        public ParserDelete(string command, Table table)
        {
            this.command = command;
            this.table = table;
            Parse();
        }

        private void Parse()
        {
            command.TrimEnd(',');
            string logicEntries = "";

            //Убираем у команды where и название таблицы
            bool whereIsFound = false;

            for (int i = 0; i < command.Length - 7; i++)
            {
                if (whereIsFound)
                    logicEntries += command[i].ToString();
                else
                {
                    if (command.Substring(i, 7).ToLower() == " where ") whereIsFound = true;
                }
            }
            if (whereIsFound == false) logicEntries = "True";
        }

        public string GetResult(string command)
        {
            ParserWhere parserWhere = new ParserWhere(table, command);
            List<Entry> entries = parserWhere.GetResult();

            return "Изменено " + table.Delete(entries) + "строк.\n"; //Результирующая строка

        }
    }
}
