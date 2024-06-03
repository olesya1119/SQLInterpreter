using SQLInterpreter.Parsers;
using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.Commands
{
    internal class TruncateCommand: IParser
    {
        public string GetResult(Table table,string args)
        {
            if (table == null) throw new ArgumentNullException("Нет открытых таблиц");
            try
            {  
                //tableName = tableName.TrimEnd(';');
                //Table table = new Table(tableName + ".dbf");
                table.Truncate();
                return "SQL>> Строки успешно удалены.";
                
            }catch(Exception ex)
            {
                throw new ArgumentException("Нет такой таблицы (файла)");
            }
        }
    }
}
