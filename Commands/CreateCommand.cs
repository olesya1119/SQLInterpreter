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

        private (string,string[]) Parse(string sqlCommand)
        {
            //string pattern = @"CREATE TABLE (\w+)\s*\(([^;]+)\);";
            //Match match = Regex.Match(sqlCommand, pattern, RegexOptions.IgnoreCase);

            //if (!match.Success)
            //{
            //    throw new ArgumentException("Неверная SQL команда");
            //}

            int index = sqlCommand.IndexOf(' ');
            //string tableName = match.Groups[1].Value;
            //string fieldsPart = match.Groups[2].Value;

            //Разделить строку на две части
            string tableName = sqlCommand.Substring(0, index);
            string fieldsPart = sqlCommand.Substring(index + 1);
            fieldsPart = fieldsPart.TrimEnd(';');
            fieldsPart = fieldsPart.Trim('(', ')');




            string[] fields = fieldsPart.Split(new string[] { ", " }, StringSplitOptions.None);
            //Console.WriteLine(tableName);
            //foreach(var  field in fields)
            //{
            //    Console.WriteLine(field);
            //}
            return (tableName, fields);
        }


        private DbfField ParseField(string field)
        {
            //DbfField newField=new DbfField()
            string name;
            char type;
            int offset = 0;
            byte size=0;
            byte accuracy=0;
            var parts = field.Split(' ');
            name = parts[0];
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
 
        public void Create(string args)
        {
            string tableName=Parse(args).Item1;
            var fields = Parse(args).Item2;
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
            }
            
        }
    }
}
