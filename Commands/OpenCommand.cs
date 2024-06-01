﻿using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.Commands
{
    internal class OpenCommand
    {
        public Table Open(string tableName)
        {
            try
            {
                return new Table(tableName+".dbf");

            }catch (Exception ex)
            {
                throw new ArgumentException("Не удалось открыть таблицу");
            }
        }
    }
}
