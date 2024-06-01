using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SQLInterpreter.Select
{
    public class ParserSelect
    {
        private string command;
        private Table table;
        private IActivity activity;

        private List<string> fieldsName;


        public ParserSelect(string command, Table table) {
            this.command = command;
            this.table = table;
            activity = new ActivitySelect();

            Parse();
        }

        private void Parse()
        {
            bool fromIsFound = false;
            string field = "", logicEntries = "";

            //Отделили часть с названием полей от логического запроса
            for (int i = 0; i < command.Length - 6; i++)
            {

                if (fromIsFound)
                    logicEntries += command[i + 5].ToString();

                else
                {
                    if (command.Substring(i, 6).ToLower() == " from ") fromIsFound = true;
                    field += command[i].ToString();
                }
            }

            logicEntries += command[command.Length - 1];

            //Разделяем названия полей на строковый список
            fieldsName = field.Split(new char[] { ',', ' ' }).ToList();
            command = logicEntries;
        }

        public string GetResult()
        {
            ParserWhere parserWhere = new ParserWhere(table, command);
            LogicEntries logicEntries = parserWhere.GetResult();
            List<Entry> entries = table.RunForArray(activity, logicEntries);

            List<List<string>> selectedEntries = Select(entries, fieldsName);
            return GetResultString(selectedEntries); //Результирующая строка

        }
        /*
        private string GetResultString(List<List<string>> table)
        {
            string s = "";

            for (int i = 0; i < table.Count; i++)
            {
                for (int j = 0; j < table[i].Count; j++)
                    s += table[i][j].ToString() + " ";
            }
            s += "\n";
            return s;
        }*/

        
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
        


        public List<List<string>> Select(List<Entry> entries, List<string> fieldsName)
        {

            List<DbfField> fields = entries[0].Header.Fields; //Список полей
            List<int> indexs = new List<int>() { };
            int index;

            List<string> fieldsNameString = new List<string>() { };//Список имен полей в строком виде
            List<List<string>> stringEntries = new List<List<string>> { };//Возвращаемый список

            //Запишем названия полей в строковый список
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].Name != "") fieldsNameString.Add(fields[i].Name);

            }

            if (fieldsName.Count > 0 && fieldsName[0] == "*") //Если берем все поля
            {

                //Добавляем заголовок
                stringEntries.Add(fieldsNameString);


                //Добавляем сами записи
                for (int i = 0; i < entries.Count; i++)
                {
                    stringEntries.Add(EntryToStringList(entries[i]));
                }
            }

            else //Если берем какие-то конкретные
            {
                stringEntries.Add(new List<string> { });
                //Сначала запомним индексы полей, которые нам надо вернуть, а так же проверим что они существуют            
                for (int i = 0; i < fieldsName.Count; i++)
                {
                    if (fieldsName[i] == "") continue;

                    index = fieldsNameString.IndexOf(fieldsName[i]);
                    if (index == -1) throw new Exception("Нет такого поля");
                    indexs.Add(index);
                    stringEntries[0].Add(fieldsName[i]);
                }


                //Добавляем сами записи
                for (int i = 0; i < entries.Count; i++)
                {
                    stringEntries.Add(EntryToStringList(entries[i], indexs));
                }
            }
            return stringEntries;
        }


        /// <summary>
        /// Перевод запись в строковый спискок и возращает только элементы указанные в списке индексов
        /// </summary>
        public List<string> EntryToStringList(Entry entry, List<int> indexs)
        {
            List<string> entryList = new List<string>() { };
            List<DbfField> fields = entry.Header.Fields; //Список полей
            List<byte> data = new List<byte> { };


            for (int i = 0; i < indexs.Count; i++)
            {
                for (int j = fields[indexs[i]].Offset; j < fields[indexs[i]].Offset + fields[indexs[i]].Size; j++)
                {
                    data.Add(entry.GetByte()[j]);
                }
                entryList.Add(Encoding.ASCII.GetString(data.ToArray()).Split('\0')[0]);
                data.Clear();
            }
            return entryList;
        }

        /// <summary>
        /// Перевод запись в строковый спискок
        /// </summary>
        private List<string> EntryToStringList(Entry entry)
        {
            List<string> entryList = new List<string>() { };
            List<DbfField> fields = entry.Header.Fields; //Список полей
            List<byte> data = new List<byte> { };


            for (int i = 0; i < fields.Count; i++)
            {
                for (int j = fields[i].Offset; j < fields[i].Offset + fields[i].Size; j++)
                {
                    data.Add(entry.GetByte()[j]);
                }
                entryList.Add(Encoding.ASCII.GetString(data.ToArray()));
                data.Clear();
            }
            return entryList;
        }
    }
}
