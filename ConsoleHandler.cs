﻿using SQLInterpreter.Commands;
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
            DropCommand dropCommand = new DropCommand();
            MainParser mainParser = new MainParser();
            while (true)
            {

                try
                {
                    Console.Write("SQL>>");
                    var str = Console.ReadLine();
                    mainParser.Parse(str);
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }         
    }
}

