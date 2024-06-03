using SQLInterpreter.Parsers;
using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.Commands
{
    internal class DropCommand
    {
        public string GetResult(string args)
        {
            string tableName = args.Remove(0,args.IndexOf("TABLE", StringComparison.OrdinalIgnoreCase)+5);
            tableName = tableName.TrimEnd(';');
            tableName = tableName.Trim();
            if (File.Exists(tableName + ".dbt"))
            {
                File.Delete(tableName + ".dbt");
            }

            if (File.Exists(tableName + ".dbf"))
            {
                File.Delete(tableName + ".dbf");
            }
            else throw new ArgumentException("Нет такой таблицы (файла)");

            return "Файлы успешно удалены.";
        }
    }
}
