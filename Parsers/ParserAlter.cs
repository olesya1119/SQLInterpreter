using SQLInterpreter.Parsers;
using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.Commands
{

    /// <summary>
    ///  Класс для парсинга команд типа ALTER
    /// </summary>
    internal class ParserAlter: IParser
    {
        private string tableName;

        /// <summary>
        ///  Удаляет столбец из таблицу.
        /// </summary>
        /// <param name="args"> Аргументы команды (имя столбца) </param>
        /// <returns>  </returns>
        private void RemoveColumn(string args)
        {
            args = args.TrimEnd(';');
            args = args.Trim();
            Table table = new Table(tableName);
            try { 
            table.RemoveColumn(args);
            }
            catch {
                throw new Exception("Поле с именем " + args + " не найдено.");
            }
        }

        /// <summary>
        ///  Добавляет новый столбец в таблицу.
        /// </summary>
        /// <param name="args"> Аргументы команды (имя столбца и его тип) </param>
        /// <returns>  </returns>
        private void AddColumn(string args)
        {
            args = args.TrimEnd(';');
            args = args.Trim();
            
            //args = args.Remove(args.Length-1,1);
            DbfField newField = CreateCommand.ParseField(args);
            Table table = new Table(tableName);
            try
            {
                table.AddColumn(newField);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        /// <summary>
        ///  Переименовывает столбец.
        /// </summary>
        /// <param name="args"> Аргументы команды </param>
        /// <returns>  </returns>
        private void RenameColumn(string args)
        {
            args = args.TrimEnd(';');
            args = args.Trim();
            var parts  = args.Split();
            string oldName = parts[0];
            string newName = parts[1];
            Table table = new Table(tableName);
            try
            {
                table.RenameColumn(oldName, newName);
            }catch(Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        ///  Обновляет заголовок столбца с привидением типа.
        /// </summary>
        /// <param name="args"> Аргументы команды </param>
        /// <returns>  </returns>
        private void UpdateColumn(string args)
        {
            Table table = new Table(tableName);
            args = args.TrimEnd(';');
            
                DbfField newField = CreateCommand.ParseField(args);
                table.UpdateColumn(newField);
            
            
        }



        /// <summary>
        ///  Определяет конкретный тип команды типа ALTER
        /// </summary>
        /// <param name="args"> Аргументы команды </param>
        /// <returns> Результат выполнения команды </returns>
        public string GetResult(Table table, string args)
        {

            if (table == null) throw new ArgumentNullException("Нет открытых таблиц.");
            tableName = table.Name;
           
            

            //удаляем слово COLUMN
            int indexOfN = args.IndexOf("COLUMN", StringComparison.OrdinalIgnoreCase);
            args = args.Remove(0, indexOfN+7);
            args = args.Trim();

            int indexOfFirstSpace = args.IndexOf(' ');
            string commandName = args.Substring(0, indexOfFirstSpace);
            commandName = commandName.ToLower();

           
            string field = args.Substring(indexOfFirstSpace + 1);


            if (commandName == "add") {
                AddColumn(field);
            }

            if(commandName == "remove")
            {
                RemoveColumn(field);
            }

            if(commandName == "rename")
            {
                RenameColumn(field);
            }

            if (commandName == "update")
            {
                UpdateColumn(field);
            }

            return "Команда ALTER " + commandName.ToUpper() + " успешно выполнена";

        }
    }
}
