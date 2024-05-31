using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.Select
{
    public class ParserSelect
    {
        private string command;
        private Table table;

        private List<string> fieldsName;
        private string logicEntries;


        public ParserSelect(string command, Table table) {
            this.command = command;
            this.table = table;
            Parse();
        }

        private void Parse()
        {
            command.TrimEnd(',');
            bool fromIsFound = false;
            string field = "", booleanExpression = "";

            //Отделили часть с названием полей от логического запроса
            for (int i = 0; i < command.Length - 6; i++)
            {

                if (fromIsFound)
                    booleanExpression += command[i].ToString();

                else
                {
                    if (command.Substring(i, 6).ToLower() == " from ") fromIsFound = true;
                    field += command[i].ToString();
                }  
            }

            //Разделяем названия полей на строковый список
            fieldsName =  field.Split(new char[]{ ',', ' '}).ToList();

            //Убираем у команды where и название таблицы
            bool whereIsFound = false;

            for (int i = 0; i < booleanExpression.Length - 7; i++)
            {
                if (whereIsFound)
                    logicEntries += booleanExpression[i].ToString();
                else
                {
                    if (booleanExpression.Substring(i, 7).ToLower() == " where ") whereIsFound = true;
                }
            }

            if (whereIsFound == false) logicEntries = "True";
        }
        
        public string GetResult(string command)
        {
            ParserWhere parserWhere = new ParserWhere(table, command);
            List<Entry> entries = parserWhere.GetResult();

            List<List<string>> selectedEntries = table.Select(entries, fieldsName);
            return GetResultString(selectedEntries); //Результирующая строка

        }

        private string GetResultString(List<List<string>> table) {
            string result = "";

            // Ширина каждой колонки таблицы
            int[] columnWidths = new int[table[0].Count];

            // Вычисление максимальной ширины для каждой колонки
            for (int i = 0; i < table[0].Count; i++)
            {
                foreach (List<string> row in table)
                {
                    if (row[i].Length > columnWidths[i])
                    {
                        columnWidths[i] = row[i].Length;
                    }
                }
            }

            // Вывод заголовка таблицы
            foreach (string column in table[0])
            {
                result += "| ";
                result += column.PadRight(columnWidths[table[0].IndexOf(column)]);
                result += " ";
            }
            result += "|\n";

            // Вывод разделителя
            for (int i = 0; i < table[0].Count; i++)
            {
                result += "+";
                result += new string('-', columnWidths[i] + 2);
            }
            result += "+\n";

            // Вывод данных
            for (int i = 1; i < table.Count; i++)
            {
                foreach (string column in table[i])
                {
                    result += "| ";
                    result += column.PadRight(columnWidths[table[i].IndexOf(column)]);
                    result += " ";
                }
                result += "|\n"; 
            }

            // Вывод разделителя
            for (int i = 0; i < table[0].Count; i++)
            {
                result += "+";
                result += new string('-', columnWidths[i] + 2);
            }
            result += "+\n";

            return result;
        }
    }
}
