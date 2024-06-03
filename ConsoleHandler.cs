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

            MainParser mainParser = new MainParser();
            while (true)
            {
                Console.Write("SQL>>");
                string request = "";
                while (request.IndexOf(';') == -1 || request == string.Empty) { request += Console.ReadLine(); }

                request = request.Trim();
                if (request[request.Length - 1] != ';')
                {
                    Console.WriteLine("SQL>>" + "После ; обнаруженены символы - \"" + request.Substring(request.IndexOf(';') + 1) + "\"");
                    continue;
                }

                try
                {
                    Console.WriteLine("SQL>>" + mainParser.Parse(request));
                }
                catch (Exception e)
                {
                    Console.WriteLine("SQL>>" + e.Message);
                }

            }

        }
    }         
}


