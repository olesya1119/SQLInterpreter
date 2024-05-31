using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    public class ParserUpdate
    {
        private Table _table;
        private List<string> _command = new List<string>();
        public List<string> Command { get => _command; }
        public ParserUpdate(Table table, string commandString)
        {
            commandString = commandString.Trim().TrimEnd(';');
            Console.WriteLine(commandString.Split(' ').Length);
            _command.Add(commandString.Split(' ')[1]); // Добавили название таблицы
            int setIndex = -1, whereIndex = -1;
            for (int i = 8; i < commandString.Length - 7; i++)
            {
                if (commandString.Substring(i, 5).ToLower() == " set ") setIndex = i + 5;
                if (commandString.Substring(i, 7).ToLower() == " where ") whereIndex = i;
            }
            if (setIndex == -1 || whereIndex == -1) throw new Exception("set и/или where не найден(ы)");
            List<Entry> entries = new ParserWhere(table, commandString).GetResult();
            string argumentsWithData = commandString.Substring(setIndex, whereIndex - setIndex), buf = "";
            bool nowIsString = false, nowIsArgument = true;
            for (int i = setIndex; i < whereIndex; i++)
            {
                if (nowIsArgument)
                {
                    if (commandString[i] == '=')
                    {
                        _command.Add(buf);
                        buf = "";
                        nowIsArgument = false;
                        continue;
                    }
                    if (commandString[i] != ' ' && commandString[i] != ',') buf += commandString[i];
                }
                else
                {
                    if (nowIsString)
                    {
                        if (commandString[i] == '\"')
                        {
                            buf += '\"';
                            _command.Add(buf);
                            buf = "";
                            nowIsString = false;
                            nowIsArgument = true;
                            continue;
                        }
                        else buf += commandString[i];
                    }
                    else
                    {
                        if (commandString[i] == '\"')
                        {
                            buf += '\"';
                            nowIsString = true;
                            continue;
                        }
                        if (commandString[i] != ' ') buf += commandString[i];
                        if (commandString[i] == ',')
                        {
                            _command.Add(buf);
                            buf = "";
                            nowIsArgument = true;
                        }
                    }
                }
                if (i == whereIndex - 1) _command.Add(buf);
            }
            _table = table;
        }
    }
}
