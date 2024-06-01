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
        private Table currentTable = null;
        public void Parse(string str) {
            

            int index = str.IndexOf(' ');
            string command = str.Substring(0, index);
            str = str.Remove(0,index+1);

            if (command.Equals("OPEN"))
            {
                try
                {
                    OpenCommand openCommand = new OpenCommand();
                    currentTable = openCommand.Open(str);
                    Console.WriteLine("SQL>>Таблица {0} открыта",currentTable.Name);
                }
                catch(Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }

            if (command.Equals("CLOSE"))
            {
                Console.WriteLine("SQL>>Таблица закрыта");
                currentTable = null;
            }

            if (command.Equals("SELECT"))
            {
                if (currentTable != null)
                {
                    ParserSelect parserSelect = new ParserSelect(str, currentTable);
                    Console.WriteLine(parserSelect.GetResult());
                }else Console.WriteLine("Нет открытых таблиц.");
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
