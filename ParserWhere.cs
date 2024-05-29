using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SQLInterpreter
{

    public class ParserWhere
    {
        List<string> command;
        Table table;
        

        //	Список и приоритет операторов
        private Dictionary<string, int> operationPriority = new Dictionary<string, int> {
            {"(", 0},
            {"or", 1},
            {"xor", 1},
            {"and", 2},
            {"not", 3},
            {"<", 4},
            {"<=", 4},
            {">", 4},
            {">=", 4},
            {"=", 4},
            {"<>", 4},
        };

        public ParserWhere(Table table, string commandString) {
            command = GetCommand(commandString); //Преобразовали строковую команду в массив
            this.table = table;          
        }

        public List<Entry> GetResult()
        {
            List<string> posfix = GetPostfixForm(); //Получили постфиксную форму    
            LogicEntries logicEntries = new LogicEntries(posfix);
            return table.Where(logicEntries);
        }


        private List<string> GetCommand(string s)
        {
            string commandString = s;
            List<string> command = new List<string> { };
            //Раставим пробелы возле всех операций, кроме логических связок

            int index = 0; //Индекс рассматриваемого элемента


            while (index < commandString.Length)
            {
                if ((commandString[index] == '>' || commandString[index] == '<') && (commandString[index + 1] == '>' || commandString[index + 1] == '<' || commandString[index + 1] == '='))
                {
                    commandString = commandString.Substring(0, index) + " " + commandString[index] + commandString[index + 1] + " " + commandString.Substring(index + 2, commandString.Length - index - 2);
                    index += 2;
                }

                else if (commandString[index] == '>' || commandString[index] == '<' || commandString[index] == '(' || commandString[index] == ')' || commandString[index] == '=')
                {
                    commandString = commandString.Substring(0, index) + " " + commandString[index] + " " + commandString.Substring(index + 1, commandString.Length - index - 1);
                    index++;
                }

                index++;
            }


            string[] stringArray1 = commandString.Split('\"'), stringArray2;
            index = 1;

            for (int i = 0; i < stringArray1.Length; i++)
            {
                if (i == index)
                {
                    if (stringArray1[i] != "")
                        command.Add("\"" + stringArray1[i] + "\"");
                    index += 2;
                }
                else
                {       
                    stringArray2 = stringArray1[i].Split(' ');
                    for (int j = 0; j < stringArray2.Length; j++)
                        if (stringArray2[j] != "")
                            command.Add(stringArray2[j].ToLower());
                }
            }

            return command;
        }


        private List<string> GetPostfixForm()
        {
            Stack<string> stack = new Stack<string>();//Операторы
            List<string> postfix = new List<string> { }; //Выходной список

            int openParentheses = 0, closeParentheses = 0; //Количество открытых и закрытых скобок

            for (int i = 0; i < command.Count; i++)
            {
                if (command[i] == "(") //Если открывающаяся скобка 
                {
                    stack.Push(command[i]); //Добавляем в стек
                    openParentheses++;
                }
                else if (command[i] == ")") //Если закрывающаяся скобка
                {
                    closeParentheses++;
                    //	Заносим в выходную строку из стека всё вплоть до открывающей скобки
                    while (stack.Count > 0 && stack.Peek() != "(")
                        postfix.Add(stack.Pop());
                    //	Удаляем открывающуюся скобку из стека
                    stack.Pop();
                }

                //	Проверяем, содержится ли элеменет в списке операторов
                else if (operationPriority.ContainsKey(command[i]))
                {
                    //	Заносим в выходную строку все операторы из стека, имеющие более высокий приоритет
                    while (stack.Count > 0 && (operationPriority[stack.Peek()] >= operationPriority[command[i]]))
                        postfix.Add(stack.Pop());
                    //	Заносим в стек оператор
                    stack.Push(command[i]);
                }

                //Иначе это значение или поле
                else
                {
                    postfix.Add(command[i]);
                }
            }

            //	Заносим все оставшиеся операторы из стека в выходную строку
            foreach (string op in stack)
                postfix.Add(op);

            if (openParentheses != closeParentheses) throw new Exception("Синтаксическая ошибка");
            return postfix;

        }
    }
}
