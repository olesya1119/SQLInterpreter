using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SQLInterpreter.Types
{
    internal static class NumberStringCheck
    {

        /// <summary>
        /// Приверяет числовую строку на соответствие значения типу поля
        /// </summary>
        /// <param name="numberString">Входная строка</param>
        /// <param name="length">Длина строки</param>
        /// <param name="accuracy">Требуемая точность</param>
        /// <returns> true если строка  соответствует значению типа поля, false иначе</returns>
        public static bool IsValidNumberString(string numberString, int length, int accuracy)
        {

           if(accuracy == 0)
            {
                string pattern = @"^[0-9]+$";
                return Regex.IsMatch(numberString, pattern);
            }
            else if(accuracy > 0)
            {
                string pattern = @"^[0-9]*\.?[0-9]*$";
                return Regex.IsMatch(numberString, pattern);
            }
            return false;
           
        }

        /// <summary>
        /// Приводит числовую строку к нужному формату при приведении типа
        /// </summary>
        /// <param name="numberString">Входная строка </param>
        /// <param name="length">Длина строки</param>
        /// <param name="accuracy">Требуемая точность</param>
        /// <returns> Строку в нужном формате </returns>
        public static string FormatString(string numberString, int length, int accuracy)
        {
            numberString = numberString.TrimEnd('\0');

            // Ищем позицию точки, если она есть
            int dotIndex = numberString.IndexOf('.');

            if (dotIndex != -1)
            {
                // Если точность 0, удаляем всё после точки
                if (accuracy == 0)
                {
                    numberString = numberString.Substring(0, dotIndex);
                }
                else
                {
                    // Иначе обрезаем до нужного количества знаков после точки
                    int endIndex = Math.Min(dotIndex + accuracy + 1, numberString.Length);
                    numberString = numberString.Substring(0, endIndex);
                }
            }

            // Обрезаем строку до нужной длины, если она длиннее
            if (numberString.Length > length)
            {
                numberString = numberString.Substring(0, length);
            }

            // Заполняем оставшиеся символы '\0' до нужной длины
            if (numberString.Length < length)
            {
                if (accuracy > 0) { 
                    numberString += '.';
                    for(int i=0; i< accuracy; i++) numberString += '0';
                }
                numberString = numberString.PadRight(length, '\0');
            }

            return numberString;
        }
    }
}
