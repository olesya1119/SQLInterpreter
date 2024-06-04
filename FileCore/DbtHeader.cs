using SQLInterpreter.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace SQLInterpreter.Properties.FileCore
{
    public class DbtHeader
    {
        private uint _nextFreeBlock; // Следующий пустой блок
        public uint NextFreeBlock { get => _nextFreeBlock; }

        public DbtHeader()
        {
            _nextFreeBlock = 2;
        }

        public DbtHeader(uint nextFreeBlock)
        {
            _nextFreeBlock = nextFreeBlock;
        }

        /// <summary>
        /// Меняет заголовок в зависимости от размера новых данных
        /// </summary>
        /// <param name="data">Новые данные</param>
        public void ChangeHeader(byte[] data)
        {
            _nextFreeBlock += (uint)(data.Length / Constants.blockSize + ((data.Length % Constants.blockSize > 0) ? 1 : 0));
        }
    }
}