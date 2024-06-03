using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.Commands
{
    internal class InsertCommand
    {
        
        private (string, string[], string[]) Parse(string args)
        {
            int startInto = args.IndexOf("INTO ", StringComparison.OrdinalIgnoreCase) + "INTO ".Length;
            int endInto = args.IndexOf(')');
            string intoPart = args.Substring(startInto, endInto - startInto);
            var parts = intoPart.Split('(');
            string tableName = parts[0];
            tableName=tableName.Replace(" ", "");
            //parts[0].TrimEnd(' ');
            var fieldsArgs = parts[1].Split(',');
            for(int i = 0;i<fieldsArgs.Length;i++)
            {
                fieldsArgs[i] = fieldsArgs[i].Replace(" ", "");
            }

            int startValues = args.IndexOf("VALUE", StringComparison.OrdinalIgnoreCase) + "VALUE".Length;
            int endValues = args.LastIndexOf(')');
            string valuesPart = args.Substring(startValues, endValues - startValues);   
            valuesPart.Trim('(', ')');
            var valuesArgs = valuesPart.Split(',');
            for( int i = 0;i<valuesArgs.Length;i++) {
                valuesArgs[i]= valuesArgs[i].Replace("\"", string.Empty);
                valuesArgs[i] = valuesArgs[i].Replace("(", string.Empty);
                valuesArgs[i] = valuesArgs[i].Replace(")", string.Empty);   
                valuesArgs[i] = valuesArgs[i].TrimStart(' ');
            }
          
            return (tableName, fieldsArgs, valuesArgs);

        }
        public void Insert(string args)
        {
            var entryArgs = Parse(args);
            string tableName = entryArgs.Item1;
            Table table = new Table(tableName+".dbf");
            try
            {
                table.AddEntry(entryArgs.Item2, entryArgs.Item3);
            }catch(Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
           
        }
    }
}
