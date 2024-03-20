using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    /// <summary>
    /// Осуществляет общение с пользователем через консоль
    /// </summary>
    internal class ConsoleUserInterface : IUserInterface
    {
        public string Input()
        {
            return Console.ReadLine();
        }

        public void Output(string message)
        {
            Console.WriteLine(message);
        }
    }
}
