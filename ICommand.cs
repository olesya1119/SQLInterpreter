using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    /// <summary>
    /// Описывает команды
    /// </summary>
    internal interface ICommand
    {
        string TableName { get; set; }
        string Arguments { get; set; }

        /// <summary>
        /// Исполнение команды
        /// </summary>
        void Execute();
    }
}
