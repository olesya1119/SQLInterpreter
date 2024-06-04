using SQLInterpreter.Parsers;
using SQLInterpreter.Properties.FileCore;
using SQLInterpreter.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SQLInterpreter.Select
{
    public class ParserSelect : IParser
    {
        private Table _table;
        private IActivity _activity;

        /// <summary>Названия полей, которые нужно вывести в таблицу</summary>
        private List<string> _fieldsNameForReturn;

        /// <summary>Логическое выражение для WHERE</summary>
        private string _logicalExpression;

        /// <summary>
        /// Создание экземпляра парсера SELECT
        /// </summary>
        /// <param name="command">Запрос без ключевого слова SELECT</param>
        /// <param name="table">Таблица, в которой нужно искать значения</param>
        public ParserSelect() {
            _activity = new ActivitySelect();
            _logicalExpression = "";
        }

        /// <summary>
        /// Вычисление результаты запроса SELECT
        /// </summary>
        /// <returns>Таблица для вывода</returns>
        public string GetResult(Table table, string args)
        {
            _table = table;
            ParserWhere parserWhere = new ParserWhere(table, args);
            LogicEntries logicEntries = parserWhere.GetResult();
            List<Entry> entries = table.RunForArray(_activity, logicEntries);
          
            Parse(args);

            List<List<string>> selectedEntries = Select(entries, _fieldsNameForReturn);
            return GetResultString(selectedEntries); //Результирующая строка

        }
      

        /// <summary>
        /// Парсер команды SELECT. Разделяет входящий запрос на значния возращаемых полей и логическое выражение WHERE
        /// </summary>
        /// <param name="command">Запрос без ключевого слова SELECT</param>
        /// <exception cref="Exception"></exception>
        private void Parse(string command)
        {          
            bool fromIsFound = false; //Найдено ли ключевое слово FROM
            string fields = ""; //Значения полей (то что идет то from)

            //Отделили часть с названием полей от логического запроса
            for (int i = 0; i < command.Length - 6; i++)
            {
                if (fromIsFound)
                    _logicalExpression += command[i + 5].ToString();

                else
                {
                    if (command.Substring(i, 6).ToLower() == " from ") fromIsFound = true;
                    fields += command[i].ToString();
                }
            }

            if (!fromIsFound) throw new Exception("Синтаксическая ошибка. Не найдено ключевое слово FROM в запросе.");


            //Разделяем названия полей на строковый список
            _fieldsNameForReturn = fields.Split(new char[] { ',', ' ' }).ToList();

            //Удаляем пустые элементы, если такие есть
            while (_fieldsNameForReturn.Remove("")) ;
        }


     
        /// <summary>
        /// Выбирает и возвращает только нужные значения
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="fieldsName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private List<List<string>> Select(List<Entry> entries, List<string> fieldsName)
        {
            if (entries.Count == 0) return new List<List<string>> { };
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


                //Теперь добавим сами записи
                List<List<string>> entriesMatrix = EntryToString.EntryListToStringMatrix(entries, _table.Name);

                for (int i = 0; i < entriesMatrix.Count; i++)
                {
                    stringEntries.Add(entriesMatrix[i]);
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

                //Теперь добавим сами записи
                List<List<string>> entriesMatrix = EntryToString.EntryListToStringMatrix(entries, _table.Name, indexs);

                for (int i = 0; i < entriesMatrix.Count; i++)
                {
                    stringEntries.Add(entriesMatrix[i]);
                }


            }
            return stringEntries;
        }





        /// <summary>
        /// Красивый вывод таблицы
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private string GetResultString(List<List<string>> table)
        {
            if (table.Count == 0) return "";
            string result = "\n";

            // Ширина каждой колонки таблицы
            int[] columnWidths = new int[table[0].Count];

            //Высота строк
            int[] rowHeight = new int[table.Count];

            // Вычисление максимальной ширины для каждой колонки
            for (int i = 0; i < table[0].Count; i++)
            {
                foreach (List<string> row in table)
                {
                    if (row[i].Length > columnWidths[i])
                    {
                        columnWidths[i] = row[i].Length;
                    }
                    if (row[i].Length > 15)
                    {
                        columnWidths[i] = 15;
                    }
                }
            }

            //Вычисление высоты каждой строки
            for (int i = 0; i < table.Count; i++)
            {
                for (int j = 0; j < table[i].Count; j++)
                {
                    if (table[i][j].Length > 15)
                    {
                        //Добавим в строку переносы строк
                        for (int k = 15; k < table[i][j].Length; k += 15)
                        {
                            table[i][j] = table[i][j].Insert(k, "\n");
                        }
                    }
                    rowHeight[i] = Math.Max(table[i][j].Length / 15 + 1, rowHeight[i]);
                }
            }

            // Вывод разделителя
            for (int i = 0; i < table[0].Count; i++) result += "+" + new string('-', columnWidths[i] + 2);
            result += "+\n";

            List<string> cell;
            for (int i = 0; i < rowHeight.Length; i++) //Перебираем строки
            {
                for (int j = 0; j < rowHeight[i]; j++) //Перебираем высоты строк
                {
                    for (int k = 0; k < columnWidths.Length; k++) //Перебираем столбы
                    {
                        cell = table[i][k].Split(new char[] { '\n', '\r' }).ToList();
                        while (cell.Count < rowHeight[i]) cell.Add("");

                        result += "|" + cell[j] + new string(' ', columnWidths[k] - cell[j].Length + 2);
                    }
                    result += "|\n";
                }



                // Вывод разделителя
                for (int j = 0; j < table[0].Count; j++) result += "+" + new string('-', columnWidths[j] + 2);
                result += "+\n";
            }

            return result;


        }


    }
}
