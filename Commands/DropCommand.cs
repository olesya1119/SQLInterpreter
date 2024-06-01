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
        public void Drop(string tableName)
        {
            tableName = tableName.TrimEnd(';');
            if (File.Exists(tableName + ".dbt"))
            {
                File.Delete(tableName + ".dbf");
            }

            if (File.Exists(tableName + ".dbf"))
            {
                File.Delete(tableName + ".dbf");
            }
            else throw new ArgumentException("Нет такой таблицы (файла)");

            
        }
    }
}
