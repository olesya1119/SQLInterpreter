﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    internal class Program
    {
       

        static void Main(string[] args)
        {
            IUserInterface userInterface = new ConsoleUserInterface();

            while (true)
            {
                userInterface.Output("SQL>");
                Parser parser = new Parser();
                try
                {
                    parser.CreateQuery(userInterface.Input());
                }
                catch (Exception ex)
                {
                    userInterface.Output(ex.Message);
                }
                
            }

        }


    }
}
