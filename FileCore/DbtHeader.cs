using SQLInterpreter.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace SQLInterpreter.Properties.FileCore
{
    public class DbtHeader
    {
        private static short blockSize = 512;
        private static short headerSize = 4;
        private uint _nextFreeBlock=1; // Следующий пустой блок
        private List<DbtBlock> _blocks = new List<DbtBlock>(); // Массив блоков для хранения текста
        public short BlockSize { get => blockSize; }
        public short HeaderSize { get => headerSize; }
        public uint NextFreeBlock { get => _nextFreeBlock; }

        public DbtHeader()
        {
            _nextFreeBlock = 1;
        }

        public DbtHeader(uint nextFreeBlock)
        {
            _nextFreeBlock = nextFreeBlock;
        }

        public DbtHeader(byte[] data)
        {
            _nextFreeBlock = 1;
            AddData(data);
        }

        /// <summary>
        /// Добавляет текст, разделяя его по блокам
        /// </summary>
        /// <param name="data">Текст, который нужно записать</param>
        public void AddData(byte[] data)
        {
            uint dataLength = (uint)data.Length, countBlocks = (uint)(dataLength / blockSize + (dataLength % blockSize > 0 ? 1 : 0));
            _nextFreeBlock += countBlocks;
            for (int i = 0; i < countBlocks; i++) // Добавляем текст блоками
            {
                _blocks.Add(new DbtBlock(new ArraySegment<byte>(data, i * blockSize, (int)(i + 1 != countBlocks ? (i + 1) * blockSize : i * blockSize + dataLength % blockSize)).ToArray()));
            }
        }

        /// <summary>
        /// Удаляет блок по номеру
        /// </summary>
        /// <param name="number">Номер нужного блока (нумерация начинается с 1)</param>
        /// <returns>Обновлённый DbtHeader</returns>
        public void RemoveBlock(uint number)
        {
            if (number == 0) throw new ArgumentException("Нельзя удалить заголовок .dbt файла с помощью этого метода");
            if (number >= _nextFreeBlock) throw new ArgumentOutOfRangeException("Блока с таким номером не существует");
            _nextFreeBlock--;
            _blocks.RemoveAt((int)number - 1);
        }

        /// <summary>
        /// Возвращает текст блока по номеру (нумерация с 1)
        /// </summary>
        /// <param name="number"></param>
        /// <returns>Текст данного блока</returns>
        public byte[] GetBlockData(uint number)
        {
            if (number == 0) throw new ArgumentException("Нельзя получить текст с заголовока .dbt файла с помощью этого метода");
            if (number >= _nextFreeBlock) throw new ArgumentOutOfRangeException("Блока с таким номером не существует");
            return _blocks[(int)number - 1].Data;
        }

        /// <summary>
        /// Изменяет блок по номеру
        /// </summary>
        /// <param name="data">Текст, который вы хотите записать в этот блок</param>
        /// <param name="number">Номер нужного блока (нумерация начинается с 1)</param>
        public void UpdateBlock(byte[] data, uint number)
        {
            if (number == 0) throw new ArgumentException("Нельзя редактировать заголовок .dbt файла с помощью этого метода");
            if (number >= _nextFreeBlock) throw new ArgumentException("Этот блок ещё не записан, и его нельзя редактировать");
            if (data.Length > BlockSize) throw new ArgumentException($"Текст не может быть больше {BlockSize} байт");
            _blocks.RemoveAt((int)number - 1);
            _blocks.Insert((int)number - 1, new DbtBlock(data));
        }

        public byte[] GetByte()
        {
            byte[] data = new byte[_blocks.Count * blockSize];
            int offset = 0;
            foreach (var block in _blocks)
            {
                Buffer.BlockCopy(block.Data, 0, data, offset, block.Length + 1);
                offset += blockSize;
            }
            return data;
        }
    }
}