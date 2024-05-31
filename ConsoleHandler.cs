using SQLInterpreter.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{

    /// <summary>
    /// Класс взаимодействия с консолью
    /// </summary>
    public class ConsoleHandler
    {
        public ConsoleHandler() {
            Parser parser = new Parser();
            CreateCommand createCommand = new CreateCommand();
            OpenCommand openCommand = new OpenCommand();
            InsertCommand insertCommand = new InsertCommand();
              while (true)
                {
                try { 
                    Console.Write("SQL>>");
                    var str = Console.ReadLine();
                    //createCommand.Create(str);  
                    //openCommand.Open(str);
                    insertCommand.Insert(str);


            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
            
        }
    }
}
