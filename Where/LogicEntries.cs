using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{

    public class LogicEntries
    {
        private List<string> command;
        private List<int> fieldIndex = new List<int> { };
        private List<char> fieldTypes = new List<char> { };

        private string[] operators = {">", "<", "<>", "=", "<=", ">=", "and", "or", "xor", "not" };
        private string[] logicOperators = {"and", "or", "xor", "not" };
        private string[] comparisonOperators = {">", "<", "<>", "=", "<=", ">=",};


        public LogicEntries(List<string> postfixCommand)
        {
            command = postfixCommand;
        }

        /// <summary>
        /// Перевод запись в строковый спискок
        /// </summary>
        private List<string> EntryToStringList(Entry entry)
        {
            List<string> entryList = new List<string>() { };
            List<DbfField> fields = entry.Header.Fields; //Список полей
            List<byte> data = new List<byte> { };


            for (int i = 0; i < fields.Count; i++)
            {
                for (int j = fields[i].Offset; j < fields[i].Offset + fields[i].Size; j++)
                {
                    data.Add(entry.GetByte()[j]);
                }
                entryList.Add(Encoding.ASCII.GetString(data.ToArray()).Split('\0')[0]);
                data.Clear();
            }
            return entryList;
        }

        //Создание шаблона расчета команды
        private void CreateCalcSample(Entry entry)
        {
            List<DbfField> fields = entry.Header.Fields; //Список полей
            List<string> fieldsNameFromTable = new List<string>() { };//Список имен полей
            List<char> fieldsTypeFromTable = new List<char>() { };  //Список типов этих полей соотственно

            for (int i = 0; i < fields.Count; i++)
            {
                fieldsNameFromTable.Add(fields[i].Name);
                fieldsTypeFromTable.Add(fields[i].Type);
            }

            bool itsField = true;
            int index;
            for (int i = 0; i < command.Count; i++){
                //Если это не оператор
                if (Array.IndexOf(operators, command[i]) == -1)
                {
                    if (itsField)
                    {
                        index = Array.IndexOf(fieldsNameFromTable.ToArray(), command[i]);
                        if (index == -1) throw new Exception("Нет такого поля");
                        itsField = false;
                        fieldIndex.Add(index);
                        this.fieldTypes.Add(fieldsTypeFromTable[index]);
                    }
                    else
                    {
                        itsField = true;
                    }
                }
            } 
        }

        public bool GetResult(Entry entry)
        {
            List<string> entryList = EntryToStringList(entry);

            CreateCalcSample(entry);


            List<string> command = new List<string>(this.command);
            int index = 0;


            //Сначала вычислим операции сравнения
            for (int i = 2; i < command.Count; i++)
            {
               
                //Если это операция сравнения
                if (Array.IndexOf(comparisonOperators, command[i]) != -1)
                {
                    
                    //То левые два элеменета - название поля и его значение соотвественно
                    //Проверим случай когда значение не строка(нет кавычек), но и не число(имеет другие символы)
                    if (command[i - 1][0] != '\"')
                    {
                        for (int j = 0; j < command[i - 1].Length; j++)
                        {
                            if ("0123456789.".Contains(command[i - 1][j]) == false)
                                if (!(command[i - 1].Length == 1 && "FfTt?".Contains(command[i - 1][0]))) //И при этом это не булевая переменная
                                {
                                    throw new Exception("Синтаксическая ошибка"); 
                                }
                            
                        }              
                    }

                    //Теперь выполним операцию
                    command[i] = ComparisonExecute(entryList[fieldIndex[index]], command[i - 1], fieldTypes[index], command[i]);

                    index++;
                    //Убираем название поля и значение
                    command.RemoveAt(i-1);
                    command.RemoveAt(i-2);
                }
            }

            //Теперь вычислим логичексие операции
            Stack<bool> locals = new Stack<bool> { }; //Стек для результатов
            for (int i = 0; i < command.Count; i++)
            {
                //Если это оператор
                if (Array.IndexOf(logicOperators, command[i]) != -1)
                {
                    //Если операция унарная - not
                    if (command[i] == "not")
                    {
                        //	Проверяем, пуст ли стек: если да - задаём нулевое значение,
                        //	если нет - выталкиваем из стека значение
                        bool last = locals.Count > 0 ? locals.Pop() : false;

                        //	Получаем результат операции и заносим в стек
                        locals.Push(LogicExecute(last));
                        continue;
                    }

                    //Остальные операции
                    bool second = locals.Count > 0 ? locals.Pop() : false,
                    first = locals.Count > 0 ? locals.Pop() : false;

                    //	Получаем результат операции и заносим в стек
                    locals.Push(LogicExecute(first, second, command[i]));

                }
                else //Это значение
                {
                    locals.Push(Boolean.Parse(command[i]));
                }
            }

            return locals.Pop();
        }

        private string ComparisonExecute(string a, string b, char type, string op)
        {
            if (type == 'L') {
                if (a == "f" || a == "F" || a == "N" || a == "n") a = "f";
                if (b == "f" || b == "F" || b == "N" || b == "n") b = "f";
                if (a == "t" || a == "T" || a == "Y" || a == "y") a = "t";
                if (b == "t" || b == "T" || b == "Y" || b == "y") b = "t";
            }

            if (type == 'C')
            {
                a = a.Trim('\"');
                b = b.Trim('\"');
            }

            switch (op)
            {
                
                case "=": return (a == b).ToString();
                case "<>": return (a != b).ToString();
            }

            switch (type)
            {
                case 'N': //Если сравниваемые переменные - числа
                    double aDouble;
                    double bDouble;
                    try
                    {
                        aDouble = Double.Parse(a);
                        bDouble = Double.Parse(b);
                    }
                    catch
                    {
                        throw new NotImplementedException("Синтаксическая ошибка");
                    }
                    switch (op)
                    {
                        case ">": return (aDouble > bDouble).ToString();
                        case ">=": return (aDouble >= bDouble).ToString();
                        case "<": return (aDouble < bDouble).ToString();
                        case "<=": return (aDouble <= bDouble).ToString();
                        default: throw new Exception("Синтаксическая ошибка");
                    }
                case 'D':  //Если сравниваемые переменные - даты
                    Date aDate;
                    Date bDate;
                    aDate = new Date(a);
                    bDate = new Date(b);

                    switch (op)
                    {
                        case ">": return (Date.Comparison(aDate, bDate) == 1).ToString();
                        case ">=": return (Date.Comparison(aDate, bDate) == 1 || Date.Comparison(aDate, bDate) == 0).ToString();
                        case "<": return (Date.Comparison(aDate, bDate) == 2).ToString();
                        case "<=": return (Date.Comparison(aDate, bDate) == 2 || Date.Comparison(aDate, bDate) == 0).ToString();
                        default: throw new Exception("Синтаксическая ошибка");
                    }

                case 'C': ////Если сравниваемые переменные - строки
                    switch (op)
                    {
                        case ">": return (Comparison(a, b) == 1).ToString();
                        case ">=": return (Comparison(a, b) == 1 || Comparison(a, b) == 0).ToString();
                        case "<": return (Comparison(a, b) == 2).ToString();
                        case "<=": return (Comparison(a, b) == 2 || Comparison(a, b) == 0).ToString();
                        default: throw new Exception("Синтаксическая ошибка");
                    }
                default: throw new Exception("Синтаксическая ошибка");
            }

        
        
        }

        private bool LogicExecute(bool a) //Для унарной операции not
        {
            return !a;
        }

        private bool LogicExecute(bool a, bool b, string op) //Для унарной операции not
        {
            switch (op)
            {
                case "and": return a & b;
                case "or": return a | b;
                case "xor": return a ^ b;
                default: throw new Exception("Синтаксическая ошибка");
            }
        }


        // Если первая строка больше вернет 1, если вторая строка больше вернет 2, если равны вернет 0
        private int Comparison(string a, string b)
        {
            int index = 0;
            if (a[0] == '\"') { a.Trim('\"'); }
            if (b[0] == '\"') { b.Trim('\"'); }

            while (a[index] == b[index]) { index++; }

            if (a[index] == b[index])
            {
                if (a.Length > b.Length) return 1;
                if (b.Length > a.Length) return 2;
                return 0;
            }

            if (a[index] > b[index]) return 1;
            return 2;
            
        }



}
}



