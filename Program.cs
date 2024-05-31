using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    internal class Program
    {

        /*
        static void Main(string[] args) { 

            List<string> fieldsName = new List<string>() {"name", "age", "bithday", "ishealthy", "city" };
            List<char> fieldsType = new List<char>() { 'C', 'N', 'D', 'L', 'C' };


            //ParserWhere parser = new ParserWhere("(age = 7 or name = \"ПеТя\") AnD isHealthy = f");
            //ParserWhere parser = new ParserWhere("(age = 7 or (age = 9 and name = \"ПеТя\")) AnD IshEALthy = F");
            ParserWhere parser = new ParserWhere("name = \"Петя\" and bithday = \"14-05-2004\"");
            LogicEntries lg = new LogicEntries(parser.GetPostfixForm());
            lg.CreateCalcSample(fieldsName, fieldsType);

            foreach (var arg in parser.GetPostfixForm()) {
                Console.Write(arg + " ");
            }
            Console.WriteLine();

            Console.WriteLine(lg.GetResult(new List<string>() { "\"Петя\"", "7", "\"14-05-2004\"", "f", "\"нск\"" }));
            Console.WriteLine(lg.GetResult(new List<string>() { "\"Петя\"", "9", "\"14-05-2004\"", "f", "\"нск\"" }));
            Console.WriteLine(lg.GetResult(new List<string>() { "\"ПеТя\"", "9", "\"14-05-2004\"", "F", "\"нск\"" }));
            Console.WriteLine(lg.GetResult(new List<string>() { "\"ПеТя\"", "9", "\"14-05-2004\"", "t", "\"нск\"" }));

        }*/

        static void Main()
        {
            // Пример входных данных
            List<List<string>> table = new List<List<string>>
        {
            new List<string> { "Заголовок 1", "Загcxcxоловок 2", "Заголовxcxcxcxcxcок 3" },
            new List<string> { "Данные 1", "Данные 2", "Даxcxcxcнные 3" },
            new List<string> { "Даннcxcxxcxcxcые 4", "Данные 5", "Данные 6" },
            new List<string> { "Данные 7", "Данные 8", "Данные 9" }
        };

            PrintTable(table);
        }

        static void PrintTable(List<List<string>> table)
        {
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
                Console.Write("| ");
                Console.Write(column.PadRight(columnWidths[table[0].IndexOf(column)]));
                Console.Write(" ");
            }
            Console.WriteLine("|");

            // Вывод разделителя
            for (int i = 0; i < table[0].Count; i++)
            {
                Console.Write("+");
                Console.Write(new string('-', columnWidths[i] + 2));
            }
            Console.WriteLine("+");

            // Вывод данных
            for (int i = 1; i < table.Count; i++)
            {
                foreach (string column in table[i])
                {
                    Console.Write("| ");
                    Console.Write(column.PadRight(columnWidths[table[i].IndexOf(column)]));
                    Console.Write(" ");
                }
                Console.WriteLine("|");
            }

            // Вывод разделителя
            for (int i = 0; i < table[0].Count; i++)
            {
                Console.Write("+");
                Console.Write(new string('-', columnWidths[i] + 2));
            }
            Console.WriteLine("+");
        }
    }
}
