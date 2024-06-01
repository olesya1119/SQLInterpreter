using System;

namespace SQLInterpreter.Properties.FileCore
{
    /// <summary>
    /// для хранения статических констант
    /// </summary>
    public static class Constants
    {
        public static short blockSize = 512;
        public static short headerSize = 4;
        public static char[] Types = new[] { 'C', 'D', 'N', 'L', 'M' };
        public static byte Terminator = 0x0D;
        public static byte NoMemo = 0x03;
        public static byte Memo = 0x83;
        public static byte Delete = 0x2A;
        public static byte NoDelete = 0x20;
        public static bool IsCorrectType(char type)
        {
            foreach (var i in Types)
            {
                if (i == type) return true;
            }
            return false;
        }

        public static bool CheckType(string value, char type)
        {
            if (!IsCorrectType(type)) return false;
            switch (type)
            {
                case 'C':
                    if (value.StartsWith("\"") && value.EndsWith("\"")) return true;
                    break;
                case 'D':
                    try
                    {
                        Date d = new Date(value);
                        return true;
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                case 'N':
                    if (value.Contains(".") && value.IndexOf('.') != value.LastIndexOf('.')) return false;
                    if (value.StartsWith("-") && value.LastIndexOf('-') != 0) return false;
                    foreach (char c in value)
                    {
                        if (!("-.0123456789".Contains(c.ToString()))) return false;
                    }
                    return true;
                case 'L':
                    if (value.Length == 1 && "yntf?".Contains(value[0].ToString().ToLower())) return true;
                    return false;
                case 'M':
                    if (value.Length > 10) return false;
                    foreach (char c in value)
                    {
                        if (!("0123456789".Contains(c.ToString()))) return false;
                    }
                    return true;
            }
            return false;
        }
    }
}