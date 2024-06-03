using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.Commands
{
    internal class AlterCommand
    {
        private string tableName;

        private void RemoveColumn(string args)
        {
            args = args.TrimEnd(';');
            args = args.Trim();
            Table table = new Table(tableName + ".dbf");
            table.RemoveColumn(args);

        }

        private void AddColumn(string args)
        {
            args = args.TrimEnd(';');
            
            //args = args.Remove(args.Length-1,1);
            DbfField newField = CreateCommand.ParseField(args);
            Table table = new Table(tableName + ".dbf");
            table.AddColumn(newField);

        }

        private void RenameColumn(string args)
        {
            args = args.TrimEnd(';');
            args = args.Trim();
            var parts  = args.Split();
            string oldName = parts[0];
            string newName = parts[1];
            Table table = new Table(tableName + ".dbf");
          
            table.RenameColumn(oldName, newName);
        }

        private void UpdateColumn(string args)
        {
            Table table = new Table(tableName + ".dbf");
            args = args.TrimEnd(';');
            DbfField newField = CreateCommand.ParseField(args);
            table.UpdateColumn(newField);
        }



        public void Parse(string args, Table table = null)
        {
            if (table == null) {
                int index = args.IndexOf('E');
                args = args.Substring(index + 1);
                args = args.TrimStart();

                index = args.IndexOf(' ');

                //Разделить строку на две части
                tableName = args.Substring(0, index);
                args = args.Remove(0, index);
                args=args.TrimStart();

            }
            else
            {
                tableName = table.Name;
            }

            //удаляем слово COLUMN
            int indexOfN = args.IndexOf('N');
            args = args.Remove(0, indexOfN+1);
            args = args.Trim();

            int indexOfFirstSpace = args.IndexOf(' ');
            string commandName = args.Substring(0, indexOfFirstSpace);

           
            string field = args.Substring(indexOfFirstSpace + 1);

            if (commandName == "ADD") {
                AddColumn(field);
            }

            if(commandName == "REMOVE")
            {
                RemoveColumn(field);
            }

            if(commandName == "RENAME")
            {
                RenameColumn(field);
            }

            if (commandName == "UPDATE")
            {
                UpdateColumn(field);
            }




        }
    }
}
