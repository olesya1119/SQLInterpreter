using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    internal class Program
    {

        /*
        static void Main(string[] args) { 

            List<string> fieldsName = new List<string>() {"name", "age", "bithday", "ishealthy", "city" };
            List<char> fieldsType = new List<char>() { 'C', 'N', 'D', 'L', 'C' };


            //ParserWhere parser = new ParserWhere("(age = 7 or name = \"ПеТя\") AnD isHealthy = f");
            //ParserWhere parser = new ParserWhere("(age = 7 or (age = 9 and name = \"ПеТя\")) AnD IshEALthy = F");
            ParserWhere parser = new ParserWhere("name = \"Петя\" and bithday = \"14-05-2004\"");
            LogicEntries lg = new LogicEntries(parser.GetPostfixForm());
            lg.CreateCalcSample(fieldsName, fieldsType);

            foreach (var arg in parser.GetPostfixForm()) {
                Console.Write(arg + " ");
            }
            Console.WriteLine();

            Console.WriteLine(lg.GetResult(new List<string>() { "\"Петя\"", "7", "\"14-05-2004\"", "f", "\"нск\"" }));
            Console.WriteLine(lg.GetResult(new List<string>() { "\"Петя\"", "9", "\"14-05-2004\"", "f", "\"нск\"" }));
            Console.WriteLine(lg.GetResult(new List<string>() { "\"ПеТя\"", "9", "\"14-05-2004\"", "F", "\"нск\"" }));
            Console.WriteLine(lg.GetResult(new List<string>() { "\"ПеТя\"", "9", "\"14-05-2004\"", "t", "\"нск\"" }));

        }*/

        static void Main(string[] args)
        {
            ConsoleHandler consoleHandler = new ConsoleHandler();
        
        }




        }
}
