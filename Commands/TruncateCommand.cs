using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.Commands
{
    internal class TruncateCommand
    {
        public void Truncate(string tableName)
        {
            try
            {
                tableName = tableName.TrimEnd(';');
                Table table = new Table(tableName + ".dbf");
                table.Truncate();
            }catch(Exception ex)
            {
                throw new ArgumentException("Нет такой таблицы (файла)");
            }
        }
    }
}
