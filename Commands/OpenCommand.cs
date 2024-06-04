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

        /// <summary>
        ///  Открывает таблицу для последующей работы.
        /// </summary>
        /// <param name="tableName">Имя таблицы </param>
        /// <returns> Открытая таблица </returns>
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
