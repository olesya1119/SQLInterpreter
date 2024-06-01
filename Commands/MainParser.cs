using SQLInterpreter.Properties.FileCore;
using SQLInterpreter.Select;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.Commands
{
    internal class MainParser
    {
        public void Parse(string str) {

            int index = str.IndexOf(' ');
            string command = str.Substring(0, index);
            str = str.Remove(0,index+1);
            if (command.Equals("SELECT"))
            {
                ParserSelect parserSelect = new ParserSelect(str, new Table("table1.dbf"));
                Console.WriteLine(parserSelect.GetResult());
            }
            if (command.Equals("CREATE"))
            {
                CreateCommand createCommand = new CreateCommand();
                createCommand.Create(str);
            }
            if (command.Equals("INSERT"))
            {
                InsertCommand insertCommand = new InsertCommand();
                insertCommand.Insert(str);
            }
            if (command.Equals("DROP"))
            {
                DropCommand dropCommand = new DropCommand();
                dropCommand.Drop(str);
            }
        }

    }
}
