namespace SQLInterpreter.Properties.FileCore
{
    /// <summary>
    /// для хранения статических констант
    /// </summary>
    public static class Constants
    {
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
    }
}