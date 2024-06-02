using SQLInterpreter.Properties.FileCore;
using SQLInterpreter.Select;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.Commands
{
    internal class MainParser
    {
        private Table currentTable = null;
        public void Parse(string str)
        {

            str = str.Trim();
            //Проверям наличие точки с запятой в выражении
            if (str[str.Length - 1] != ';') throw new ArgumentException("Синтаксическая ошибка. В конце запроса ожидалось ';'");


            int index = str.IndexOf(' ');
            string command = str.Substring(0, index).ToLower();
            str = str.Remove(0, index + 1);


            if (command.Equals("open"))
            {
                try
                {

                    OpenCommand openCommand = new OpenCommand();
                    currentTable = openCommand.Open(str);
                    Console.WriteLine("SQL>>Таблица {0} открыта", currentTable.Name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (command.Equals("close"))
            {
                Console.WriteLine("SQL>>Таблица закрыта");
                currentTable = null;
            }

            if (command.Equals("select"))
            {
                if (currentTable != null)
                {
                    ParserSelect parserSelect = new ParserSelect(str, currentTable);
                    Console.WriteLine(parserSelect.GetResult());
                }
                else Console.WriteLine("Нет открытых таблиц.");
            }
            if (command.Equals("create"))
            {
                CreateCommand createCommand = new CreateCommand();
                createCommand.Create(str);
            }
            if (command.Equals("insert"))
            {
                InsertCommand insertCommand = new InsertCommand();
                insertCommand.Insert(str);
            }
            if (command.Equals("drop"))
            {
                DropCommand dropCommand = new DropCommand();
                dropCommand.Drop(str);
            }

            if (command.Equals("truncate"))
            {
                TruncateCommand truncateCommand = new TruncateCommand();
                truncateCommand.Truncate(str);
            }

            if (command.Equals("delete"))
            {
                if (currentTable != null)
                {
                    ParserDelete parserDelete = new ParserDelete(str, currentTable);
                    Console.WriteLine(parserDelete.GetResult());
                }
                else Console.WriteLine("Нет открытых таблиц.");
            }

            if (command.Equals("update"))
            {
                if (currentTable != null)
                {
                    ParserUpdate parserUpdate = new ParserUpdate(str, currentTable);
                    Console.WriteLine(parserUpdate.GetResult());
                }
                else Console.WriteLine("Нет открытых таблиц.");
            }
        }
    }
}
