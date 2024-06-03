using SQLInterpreter.Parsers;
using SQLInterpreter.Properties.FileCore;
using SQLInterpreter.Select;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SQLInterpreter.Commands
{
    internal class MainParser
    {
        private Table _table = null; //Открытая таблица
        private Dictionary<string, IParser> commands = new Dictionary<string, IParser>() {
            {"alter",    new }, 
            {"insert",   new }, 
            {"update",   new ParserUpdate()}, 
            {"delete",   new ParserDelete()}, 
            {"select",   new ParserSelect()}, 
            {"truncate", new }, 
            {"restore",  new ParserRestore()}, 
        };


        public string Parse(string request)
        {
            int index = request.IndexOf(' '); //Находим индекс конца первого слова - названия команды
            string command = request.Substring(0, index).ToLower();
            request = request.Remove(0, index + 1);

            index = request.IndexOf(' '); //Находим индекс следующего пробела с именем таблицы
            string tableName = request.Substring(0, index);


            if (command.Equals("open"))
            {
                OpenCommand openCommand = new OpenCommand();
                _table = openCommand.Open(request);
                return "Таблица" + _table.Name + "открыта";
            }
            else if (command.Equals("drop"))
            {

            }
            else if (command.Equals("close"))
            {

            }
            else if (command.Equals("exit"))
            {
                Console.WriteLine("SQL>>Работа интерперетатора SQL завершена.");
                _table = null;
                Environment.Exit(0);
            }
            else if (commands.ContainsKey(command))
            {
                tables.TryGetValue(tableName, out _table);
                return commands[command].GetResult(_table, request);
            }
            else
            {
                return "Запрос " + command + "не найден.";
            }

            

            /*
            if (command.Equals("alter"))
            {
                AlterCommand alterCommand = new AlterCommand();
                alterCommand.GetResult(currentTable,str);
            }
          
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
                insertCommand.GetResult(currentTable,str);
            }
            if (command.Equals("drop"))
            {
                DropCommand dropCommand = new DropCommand();
                dropCommand.GetResult(str);
            }

            if (command.Equals("truncate"))
            {
                TruncateCommand truncateCommand = new TruncateCommand();
                truncateCommand.GetResult(currentTable,str);
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
            } */
        }
    }
}
