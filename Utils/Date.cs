using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    public class Date
    {
        
        /// <summary>
        /// Если первая дата больше вернет 1, если вторая дата больше вернет 2, если равны вернет 0
        /// </summary>
        /// <param name="a">Первая дата</param>
        /// <param name="b">Вторая дата</param>
        /// <returns></returns>
        public static int Comparison(Date a, Date b)
        {
            if (a.year > b.year) return 1;
            else if (a.year < b.year) return 2;
            else if (a.month > b.month) return 1;
            else if (a.month < b.month) return 2;
            else if (a.day > b.day) return 1;
            else if (a.day < b.day) return 2;
            else return 0;
        }

        private int year;
        private int month;
        private int day;

        public int Year
        {
            get { return year; }
            private set
            {
                if (value >= 10000 || value < 0) throw new ArgumentException("Синтаксическая ошибка");
                year = value;
            }
        }

        public int Month
        {
            get { return month; }
            private set
            {
                if (value > 12 || value <= 0) throw new ArgumentException("Синтаксическая ошибка");
                month = value;
            }
        }

        public int Day
        {
            get { return day; }
            private set
            {
                if (month != 2 || month != 4 || month != 6 || month != 9 || month != 11)
                {
                    if (value > 31 || value <= 0) throw new ArgumentException("Синтаксическая ошибка");
                }
                else if (month != 2)
                {
                    if (value > 30 || value <= 0) throw new ArgumentException("Синтаксическая ошибка");
                }
                else 
                {
                    if (((year % 4 == 0) && (year % 100 != 0)) || (year % 400 == 0))
                    {
                        if (value > 29 || value <= 0) throw new ArgumentException("Синтаксическая ошибка");
                    }
                    if (((year % 4 == 0) && (year % 100 != 0)) || (year % 400 == 0))
                    {
                        if (value > 28 || value <= 0) throw new ArgumentException("Синтаксическая ошибка");
                    }

                }
                day = value;
            }
        }

        public Date(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
        }

        public Date(string data)
        {
            try {
                if (data.Length == 8) //Если в формате YYYYMMDD
                {
                    Year = Int32.Parse(data.Substring(0, 4));
                    Month = Int32.Parse(data.Substring(4, 2));
                    Day = Int32.Parse(data.Substring(6, 2));

                }
                else if (data.Length == 10) //Если в формате YYYY-MM-DD
                {
                    Year = Int32.Parse(data.Substring(0, 4));
                    Month = Int32.Parse(data.Substring(4, 2));
                    Day = Int32.Parse(data.Substring(6, 2));
                }
                else
                {
                    throw new Exception("Синтаксичекая ошибка.");
                }
            }
            catch {
                throw new Exception("Синтаксичекая ошибка.");
            }

        }

        public override string ToString()
        {
            return year.ToString("D4") + "-" + month.ToString("D2") + "-" + day.ToString("D2");
        }

        public override bool Equals(object obj)
        {
            return year == ((Date)obj).year && month == ((Date)obj).month && day == ((Date)obj).day;
        }


        public byte[] ToByteArray()
        {
            string s = year.ToString("D4") + month.ToString("D2") + day.ToString("D2");
            byte[] b = new byte[8];
            b = Encoding.ASCII.GetBytes(s);
            return b;
        }   
    }
}
