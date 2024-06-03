using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SQLInterpreter.Commands
{
    public class CreateCommand
    {
        private EntryVirtualArray table;
        private DbfHeader header;

        private string DeleteSpaces(string str)
        {
            return str.Trim();
        }


        private (string,string[]) Parse(string sqlCommand)
        {
            //удаляем слово TABLE
            int index = sqlCommand.IndexOf('E');
            sqlCommand = sqlCommand.Substring(index + 1);
            sqlCommand = sqlCommand.TrimStart();

            index = sqlCommand.IndexOf(' ');

            //Разделить строку на две части
            string tableName = sqlCommand.Substring(0, index);
            string fieldsPart = sqlCommand.Substring(index + 1);
            fieldsPart = fieldsPart.TrimEnd(';');//удаляем ; и скобки
            fieldsPart = fieldsPart.TrimStart('(');
            fieldsPart = fieldsPart.Remove(fieldsPart.Length-1);

            //делимна отдельные аргументы
            string[] fields = fieldsPart.Split(new string[] { ", " }, StringSplitOptions.None);
            return (tableName, fields);
        }


        public static DbfField ParseField(string field)
        {
            
            string name;
            char type;
            int offset = 0;
            byte size=0;
            byte accuracy=0;
            var parts = field.Split();
            name = parts[0];
            parts[1] = parts[1].TrimStart('(');
            type = parts[1][0];

           
            

            
            if (!Constants.IsCorrectType(type))
            {
                throw new ArgumentException("Неправильный тип поля");
            }
            if (type.Equals('M'))
            {
                size = 10;
            }
            if (type.Equals('L'))
            {
                size = 1;
            }
            if (type.Equals('D'))
            {
                size = 8;
            }
            if (type.Equals('N'))//парсим ширину и точность
            {
                int startIndex = parts[1].IndexOf('(');
                int endIndex = parts[1].IndexOf(')');
                string numbers = parts[1].Substring(startIndex + 1, endIndex - startIndex - 1);
                if (numbers.Contains(","))
                {
                    string[] splittedNumbers = numbers.Split(',');
                    size = byte.Parse(splittedNumbers[0]);
                    accuracy = byte.Parse(splittedNumbers[1]);
                } else size = byte.Parse(numbers);
            }
            else if (type.Equals('C'))//парсим только ширину
            {
                int startIndex = parts[1].IndexOf('(');
                int endIndex = parts[1].IndexOf(')');
                string number = parts[1].Substring(startIndex + 1, endIndex - startIndex - 1);
                size = byte.Parse(number);
                accuracy = 0;
            }
            else
            {
                if (parts[1].Length > 1) throw new ArgumentException("Неверный формат поля");
            }
            return new DbfField(name, type, offset, size, accuracy);

        }
 
        public string GetResult(string args)
        {
            var parts = Parse(args);
            string tableName=parts.Item1;
            var fields = parts.Item2;
            header = new DbfHeader();
           
            foreach(string field in fields)
            {
                header.AddField(ParseField(field));
            }
            if (File.Exists(tableName + ".dbf"))
            {
                throw new ArgumentException("Таблица с таким именем уже существует");
            }
            else
            {
                EntryVirtualArray table = new EntryVirtualArray(tableName + ".dbf", header);
                table.Close();
                return "Таблица " + tableName + ".dbf успешно создана.";
            }
            
        }
    }
}
