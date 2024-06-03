using SQLInterpreter.Parsers;
using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.Commands
{
    internal class ParserInsert: IParser
    {
        
        private (string, string[], string[]) Parse(string args)
        {
            int startInto = args.IndexOf("INTO ", StringComparison.OrdinalIgnoreCase) + "INTO ".Length;
            int endInto = args.IndexOf(')');
            string intoPart = args.Substring(startInto, endInto - startInto);
            var parts = intoPart.Split('(');
            string tableName = parts[0];
            tableName=tableName.Trim();
            //parts[0].TrimEnd(' ');
            var fieldsArgs = parts[1].Split(',');
            for(int i = 0;i<fieldsArgs.Length;i++)
            {
                fieldsArgs[i] = fieldsArgs[i].Trim();
            }

            int startValues = args.IndexOf("VALUE", StringComparison.OrdinalIgnoreCase) + "VALUE".Length;
            int endValues = args.LastIndexOf(')');
            string valuesPart = args.Substring(startValues, endValues - startValues);   
            valuesPart = valuesPart.Trim('(', ')');
            var valuesArgs = valuesPart.Split(',');
            for( int i = 0;i<valuesArgs.Length;i++) {
                valuesArgs[i] = valuesArgs[i].Trim();
                valuesArgs[i]= valuesArgs[i].Replace("\"", string.Empty);
                valuesArgs[i] = valuesArgs[i].Trim('(',')');
                valuesArgs[i] = valuesArgs[i].Trim();
                //valuesArgs[i] = valuesArgs[i].Replace(")", string.Empty);   
                //valuesArgs[i] = valuesArgs[i].Trim();
            }
          
            return (tableName, fieldsArgs, valuesArgs);

        }
        public string GetResult(Table table,string args)
        {
            if (table == null) throw new ArgumentNullException("Нет открытых таблиц");
            string[] fieldsHeaders=null, fieldsData=null;
            try
            {
                var entryArgs = Parse(args);
                fieldsHeaders = entryArgs.Item2;
                fieldsData = entryArgs.Item3;
            }catch(Exception ex)
            {
                throw new ArgumentException("Синтаксическая ошибка в выражении");
            }
            string tableName = table.Name;
            //Table table = new Table(tableName+".dbf");
            try
            {
                
                table.AddEntry(fieldsHeaders, fieldsData);
                return "Добавлена одна строка в таблицу "+ tableName;
            }
            catch(Exception ex)
            {
                throw new ArgumentException("Несоответствие значения типу поля");
            }
           
           
        }
    }
}
