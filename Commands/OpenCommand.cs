using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.Commands
{
    internal class OpenCommand
    {
        public Table Open(string tableName)
        {
            try
            {
                tableName = tableName.TrimEnd(';');
                return new Table(tableName+".dbf");

            }catch 
            {
                throw new ArgumentException("Не удалось открыть таблицу");
            }
        }
    }
}
