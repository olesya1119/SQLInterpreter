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
        private List<DbtBlock> _blocks = new List<DbtBlock>(); // Массив блоков для хранения текста
        public uint NextFreeBlock { get => _nextFreeBlock; }

        public DbtHeader()
        {
            _nextFreeBlock = 2;
        }

        public DbtHeader(uint nextFreeBlock)
        {
            _nextFreeBlock = nextFreeBlock;
        }

        public DbtHeader(byte[] data)
        {
            _nextFreeBlock = 2;
            WriteAllData(data);
        }

        /// <summary>
        /// Читает все данные из файла (data может быть > 512 байт)
        /// </summary>
        /// <param name="data">Текст, который нужно записать</param>
        private void WriteAllData(byte[] data)
        {
            int lastLen = data.Length % Constants.blockSize, len = data.Length / Constants.blockSize + ((lastLen > 0) ? 1 : 0);
            for (int i = 0; i < len; i++)
            {
                AddData(new ArraySegment<byte>(data, i * Constants.blockSize, i + 1 == len ? lastLen : Constants.blockSize).ToArray());
            }
        }
    

        /// <summary>
        /// Добавляет текст, разделяя его по блокам
        /// </summary>
        /// <param name="data">Текст, который нужно записать</param>
        public void AddData(byte[] data)
        {
            if (data.Length > Constants.blockSize) throw new ArgumentException($"Текст не может быть больше {Constants.blockSize} байт");
            _blocks.Add(new DbtBlock(data));
            _nextFreeBlock++;
        }

        /// <summary>
        /// Удаляет блок по номеру
        /// </summary>
        /// <param name="number">Номер нужного блока (нумерация начинается с 1)</param>
        /// <returns>Обновлённый DbtHeader</returns>
        public void RemoveBlock(uint number)
        {
            if (number == 1) throw new ArgumentException("Нельзя удалить заголовок .dbt файла с помощью этого метода");
            if (number >= _nextFreeBlock || number == 0) throw new ArgumentOutOfRangeException("Блока с таким номером не существует");
            _nextFreeBlock--;
            _blocks.RemoveAt((int)number - 2);
        }

        /// <summary>
        /// Возвращает текст блока по номеру (нумерация с 1)
        /// </summary>
        /// <param name="number"></param>
        /// <returns>Текст данного блока</returns>
        public byte[] GetBlockData(uint number)
        {
            if (number == 1) throw new ArgumentException("Нельзя получить текст с заголовока .dbt файла с помощью этого метода");
            if (number >= _nextFreeBlock) throw new ArgumentOutOfRangeException("Блока с таким номером не существует");
            return _blocks[(int)number - 2].Data;
        }

        /// <summary>
        /// Изменяет блок по номеру
        /// </summary>
        /// <param name="data">Текст, который вы хотите записать в этот блок</param>
        /// <param name="number">Номер нужного блока (нумерация начинается с 1)</param>
        public void UpdateBlock(byte[] data, uint number)
        {
            if (number == 1) throw new ArgumentException("Нельзя редактировать заголовок .dbt файла с помощью этого метода");
            if (number >= _nextFreeBlock || number == 0) throw new ArgumentException("Этот блок ещё не записан, и его нельзя редактировать");
            if (data.Length > Constants.blockSize) throw new ArgumentException($"Текст не может быть больше {Constants.blockSize} байт");
            _blocks.RemoveAt((int)number - 2);
            _blocks.Insert((int)number - 2, new DbtBlock(data));
        }

        public byte[] GetByte()
        {
            byte[] data = new byte[_blocks.Count * Constants.blockSize];
            int offset = 0;
            foreach (var block in _blocks)
            {
                Buffer.BlockCopy(block.Data, 0, data, offset, Constants.blockSize);
                offset += Constants.blockSize;
            }
            return data;
        }
    }
}