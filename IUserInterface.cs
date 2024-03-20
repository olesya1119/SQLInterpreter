using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    /// <summary>
    /// Описывает взаимодейтсвие с пользователем
    /// </summary>
    internal interface IUserInterface
    {
        /// <summary>
        /// Осущетсвляет ввод сообщения пользователем. 
        /// </summary>
        /// <returns>Введенная строка</returns>
        string Input();

        /// <summary>
        /// Осущетсвляет вывод сообщения
        /// </summary>
        /// <param name="message">Выводимое сообщение</param>
        void Output(string message);
    }
}
