using SQLInterpreter.Parsers;
using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.Commands
{
    internal class ParserTruncate: IParser
    {

        /// <summary>
        /// Физически удаляет из таблицы строки помеченные у удалению
        /// </summary>
        /// <param name="table">Таблица из которой удаляются строки </param>
        /// <param name="args">Имя таблицы </param>
        /// <returns> Результат операции </returns>
        public string GetResult(Table table,string args)
        {
            if (table == null) throw new ArgumentNullException("Нет открытых таблиц");
            try
            {  
                //tableName = tableName.TrimEnd(';');
                //Table table = new Table(tableName + ".dbf");
                table.Truncate();
                return "Строки успешно удалены.";
                
            }catch(Exception ex)
            {
                throw new ArgumentException("Нет такой таблицы (файла)");
            }
        }
    }
}
