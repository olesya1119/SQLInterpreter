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
        private string _stringCommand;
        private List<string> _command = new List<string>();
        public List<string> Command { get => _command; } //{field1, value1, field2, value2, ...}
        private IActivity _activity;
        private Table _table;
        public ParserUpdate(string command, Table table)
        {
            Parse(command);
            _activity = new ActivityUpdate(_command);
            _table = table;
            _stringCommand = command;
        }
        
        public string GetResult()
        {
            ParserWhere parserWhere = new ParserWhere(_table, _stringCommand);
            List<Entry> entries = _table.RunForArray(_activity, parserWhere.GetResult());
            return "Изменено " + entries.Count + " строк.\n"; //Результирующая строка
        }

        private void Parse(string commandString)
        {
            commandString = commandString.Trim().TrimEnd(';');
            int setIndex = -1, whereIndex = -1;
            for (int i = 8; i < commandString.Length - 7; i++)
            {
                if (commandString.Substring(i, 5).ToLower() == " set ") setIndex = i + 5;
                if (commandString.Substring(i, 7).ToLower() == " where ") whereIndex = i + 1;
            }
            if (setIndex == -1 || whereIndex == -1) throw new Exception("set и/или where не найден(ы)");
            string buf = "";
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
                        if (commandString[i] == ',')
                        {
                            _command.Add(buf);
                            buf = "";
                            nowIsArgument = true;
                            continue;
                        }
                        if (commandString[i] != ' ') buf += commandString[i];
                    }
                }
                if (i == whereIndex - 1) _command.Add(buf);
            }
        }
    }
}
