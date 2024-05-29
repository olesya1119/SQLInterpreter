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
        

        //	Список и приоритет операторов
        public Dictionary<string, int> operationPriority = new Dictionary<string, int> {
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

        public ParserWhere(string commandString) {
            command = GetCommand(commandString); //Преобразовали строковую команду в массив
            List<string> posfix = GetPostfixForm(); //Получили постфиксную форму         
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


        public List<string> GetPostfixForm()
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

        //Старая версия функции
        /*
        public string GetPostfixForm()
        {
            Stack<string> stack= new Stack<string>();//Операторы
            string postfix = ""; //Выходная строка
            int openParentheses = 0, closeParentheses = 0; //Количество открытых и закрытх скобок


            int index = 0; //Индекс элемента, который мы рассматриваем 
            string element; //Рассматриваемый элемент
            while (index < command.Length)
            {
                //Сначала рассмторим если длина рассматриваемого элемента равна 1
                element = command[index].ToString();

                //Сразу проверим является ли строковым значением
                if (command[index] == '"')
                {
                    do
                    {
                        element += command[index + 1].ToString();
                        index += 1;
                    }
                    while (command[index] != '"');
                    postfix += element + " ";
                    continue;
                }

                if (element == " ") //Если пробел, то двигаемся дальше
                {
                    index++;
                    continue;
                }

                else if (element == "(")  //Если открывающаяся скобка 
                {
                    stack.Push(element); //Добавляем в стек
                    openParentheses ++;

                    index++;
                    continue;
                }
                else if (element == ")") //Если закрывающаяся скобка
                {
                    closeParentheses ++;
                    //	Заносим в выходную строку из стека всё вплоть до открывающей скобки
                    while (stack.Count > 0 && stack.Peek() != "(")
                        postfix += stack.Pop() + " ";
                    //	Удаляем открывающуюся скобку из стека
                    stack.Pop();

                    index++;
                    continue;
                }
                else if (operationPriority.ContainsKey(element) && command[index + 1] != '=') //Иначе является операцией сравнения
                {
                    //	Заносим в выходную строку все операторы из стека, имеющие более высокий приоритет
                    while (stack.Count > 0 && (operationPriority[stack.Peek()] >= operationPriority[element]))
                        postfix += stack.Pop() + " ";
                    //	Заносим в стек оператор
                    stack.Push(element);

                    index ++;
                    continue;
                }

                Console.WriteLine(element);

                //Если элемент состоит из двух символов и следующий не пробел
                if (index + 1 < command.Length && command[index + 1] != ' ')
                    element += command[index + 1].ToString();
                else
                {
                    postfix += element + " ";

                    index++;
                    continue;
                }

                Console.WriteLine(element);

                //Проверяем, содержится ли элемент в списке операторов
                if (operationPriority.ContainsKey(element))
                {
                    //Проверяем что слева и справа от or пробел 
                    if (element == "or" && command[index - 1] == ' ' && command[index + 2] == ' ')
                    {
                        //	Заносим в выходную строку все операторы из стека, имеющие более высокий приоритет
                        while (stack.Count > 0 && (operationPriority[stack.Peek()] >= operationPriority[element]))
                            postfix += stack.Pop() + " ";
                        //	Заносим в стек оператор
                        stack.Push(element);
                        index += 2;
                        continue;
                    }
                }

                //Если элемент состоит из трех символов и следующий не пробел
                if (index + 2 < command.Length && command[index + 2] != ' ')
                    element += command[index + 2].ToString();
                else
                {
                    postfix += element + " ";
                    index+=2;
                    continue;
                }

                //Проверяем, содержится ли элмент в списке операторов
                if (operationPriority.ContainsKey(element))
                {
                    //Проверяем что слева и справа пробел
                    if (command[index - 1] == ' ' && command[index + 2] == ' ')
                    {
                        //	Заносим в выходную строку все операторы из стека, имеющие более высокий приоритет
                        while (stack.Count > 0 && (operationPriority[stack.Peek()] >= operationPriority[element]))
                            postfix += stack.Pop() + " ";
                        //	Заносим в стек оператор
                        stack.Push(element);
                        index += 3;
                        continue;
                    }
                }

                //Иначе наш элемент либо название поля, либо какое-то нестрокове значение
                index += 3;
                //Проверим что в составе элемента нет пробелов

                Console.WriteLine("|" + element + "|");
                if (element[1] == ' ' || element[2] == ' ') throw new Exception("Синтаксическая ошибка");

                //Значит добавляем символы пока не встретим пробел
                while (command[index] != ' ')
                {
                    element += command[index + 1].ToString();
                    index += 1;
                }
                postfix += element + " ";

            }

            //	Заносим все оставшиеся операторы из стека в выходную строку
            foreach (string op in stack)
                postfix += op;

            return postfix;
        }
        */
    }
}
