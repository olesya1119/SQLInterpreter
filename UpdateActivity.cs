using System;
using System.Collections.Generic;
using System.Text;
using SQLInterpreter.Properties.FileCore;

namespace SQLInterpreter
{
    public class ActivityUpdate : IActivity
    {
        private List<string> _command;

        public ActivityUpdate(List<string> command)
        {
            _command = command;
        }
        public void Do(Entry entry)
        {
            for (int i = 0; i < _command.Count; i += 2)
            {
                var field = entry.Header.GetField(_command[i]);
                if (Constants.CheckType(_command[i + 1], field.Type) &&
                    CheckSize(_command[i + 1], field.Size, field.Accuracy, field.Type))
                {
                    if (field.Type == 'C') _command[i + 1] = _command[i + 1].Trim('\"');
                    entry.Update(_command[i], Encoding.ASCII.GetBytes(_command[i + 1]));
                }
            }
        }

        private bool CheckSize(string value, byte size, byte accuracy, char type)
        {
            if (value.Length > size || (type == 'N' && value.Length - value.IndexOf('.') > accuracy)) return false;
            return true;
        }
    }
}