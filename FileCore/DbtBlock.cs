using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.FileCore
{
    internal class DbtBlock
    {
        private static short blockSize = 512;
        private byte[] _data = new byte[blockSize];
        public byte[] Data { get => _data; }
        public short Length
        { 
            get
            {
                short len = (short)(blockSize - 1);
                while (_data[len] == 0x0) len--;
                return len;
            }
        }
        public DbtBlock(byte[] data)
        {
            Buffer.BlockCopy(data, 0, _data, 0, data.Length);
        }
    }
}
