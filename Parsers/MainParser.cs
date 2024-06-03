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
        private Table openTable = null; //Открытая таблица
        private Table _table = null; //Таблица, которую пытается открыть пользователь
        private Dictionary<string, IParser> parsers = new Dictionary<string, IParser>() {

            {"alter",    new ParserAlter()}, 
            {"insert",   new ParserInsert()}, 
            {"update",   new ParserUpdate()}, 
            {"delete",   new ParserDelete()}, 
            {"select",   new ParserSelect()}, 
            {"truncate", new ParserTruncate()}, 
            {"restore",  new ParserRestore()}, 
        };

        private OpenCommand openCommand = new OpenCommand();
        private DropCommand dropCommand = new DropCommand();
        private CreateCommand createCommand = new CreateCommand();

        public string Parse(string request)
        {
            int index = request.IndexOf(' '); //Находим индекс конца первого слова - названия команды
            string command = request.Substring(0, index).ToLower();
            request = request.Remove(0, index + 1);


            if (command.Equals("open"))
            {
                openTable = openCommand.Open(request);
                return "Таблица " + openTable.Name + " открыта.";
            }
            else if (command.Equals("create"))
            {
                return createCommand.GetResult(request);
            }
            else if (command.Equals("drop"))
            {
                return dropCommand.GetResult(request);
            }
            else if (command.Equals("close"))
            {
                
                if (openTable == null) throw new Exception("Не нашлось открытых таблиц. Используйте команду OPEN для открытия.");
                try
                {
                    _table = openCommand.Open(GetTableName(command, request));
                }
                catch
                {
                    throw new Exception("Таблица с именем " + GetTableName(command, request) + "не существует. Используйте команду CREATE для создания.");
                }
                if (openTable.Name != _table.Name) throw new Exception("Таблица с именем " + _table.Name + "не открыта. Используйте команду OPEN для открытия.");
                openTable = null;
                return "Таблица " + _table.Name + "успешно закрыта.";
            }
            else if (command.Equals("exit"))
            {
                Console.WriteLine("SQL>>Работа интерперетатора SQL завершена.");
                Environment.Exit(0);
            }
            else if (parsers.ContainsKey(command))
            {
                if (openTable == null) throw new Exception("Не нашлось открытых таблиц. Используйте команду OPEN для открытия.");
                try { 
                    _table = openCommand.Open(GetTableName(command, request));
                }
                catch
                {
                    throw new Exception("Таблица с именем " + GetTableName(command, request) + "не существует. Используйте команду CREATE для создания.");
                }
                if (openTable.Name != _table.Name) throw new Exception("Таблица с именем " + _table.Name + "не открыта. Используйте команду OPEN для открытия.");
                _table = null;
                return parsers[command].GetResult(openTable, request);
            }
            throw new Exception("Запрос " + command.ToUpper() + " не найден.");
        }

        private string GetTableName(string command, string args)
        {
            string tableName;
            int index;
            
            //Имя таблицы идет сразу после этих ключивых слов
            if (Array.IndexOf(new string[] { "close", "update", "truncate", "restore" }, command) != -1)
            {
                tableName = args.Split()[0];
            }
            //Перед таблицей стоит какое-то ключевое слово - Table
            else if (Array.IndexOf(new string[] { "alter"}, command) != -1 )
            {
                index = args.IndexOf(' '); //Находим индекс конца первого слова - ключевое слово TABLE
                if (args.Substring(0, index).ToLower() != "table") throw new Exception("Синтаксическая ошибка. Не найдено ключевое слово TABLE в запросе."); 
                tableName = args.Remove(0, index);
                tableName = tableName.Trim();
                tableName = tableName.Trim(';');

                index = tableName.IndexOf(' ');
                if (index != -1) tableName = tableName.Substring(0, index); 

            }
            //Перед таблицей стоит какое-то ключевое слово - from
            else if (Array.IndexOf(new string[] {"delete"}, command) != -1)
            {
                index = args.IndexOf(' '); //Находим индекс конца первого слова - ключевое слово TABLE
                if (args.Substring(0, index).ToLower() != "from") throw new Exception("Синтаксическая ошибка. Не найдено ключевое слово FROM в запросе.");
                tableName = args.Remove(0, index);
                tableName = tableName.Trim();
                tableName = tableName.Trim(';');

                index = tableName.IndexOf(' ');
                if (index != - 1) tableName = tableName.Substring(0, index);

            }
            //Перед таблицей стоит какое-то ключевое слово - INTO
            else if (Array.IndexOf(new string[] { "insert" }, command) != -1)
            {
                index = args.IndexOf(' '); //Находим индекс конца первого слова - ключевое слово INTO
                if (args.Substring(0, index).ToLower() != "into") throw new Exception("Синтаксическая ошибка. Не найдено ключевое слово INTO в запросе."); 
                tableName = args.Remove(0, index);
                tableName = tableName.Trim();
                tableName = tableName.Trim(';');

                index = tableName.IndexOf(' ');
                if (index != -1) tableName = tableName.Substring(0, index); 
            }
            else if (Array.IndexOf(new string[] { "select"}, command) != -1){
                index = args.ToLower().IndexOf(" from "); //Находим индекс конца слова FROM
                if (index == -1) { throw new Exception("Синтаксическая ошибка. Не найдено ключевое слово FROM в запросе."); }
                tableName = args.Remove(0, index + 5);
                tableName = tableName.Trim();
                tableName = tableName.Trim(';');

                index = tableName.IndexOf(' ');
                if (index != -1) { tableName = tableName.Substring(0, index); }
            }
            else
            {
                throw new Exception("Запрос " + command.ToUpper() + " не найден.");
            }

            return tableName;

        }
    }
}
