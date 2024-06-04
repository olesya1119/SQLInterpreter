using Microsoft.SqlServer.Server;
using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.FileCore
{
    internal class DbtFile
    {
        private FileStream _stream;
        private DbtHeader _header;
        public DbtHeader Header { get => _header; }

        /// <summary>
        /// Конструктор для открытия существуещего файла или создания нового файла (если он ещё не был создан)
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        public DbtFile(string path)
        {
            if (File.Exists(path)) Open(path); // Если файл есть, то открываем его
            else Create(path); // Иначе создаём
        }

        /// <summary>
        /// Открывает файл по указанному пути
        /// </summary>
        /// <param name="path">Путь до файла</param>
        void Open(string path)
        {
            _stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Extension != ".dbt") throw new ArgumentException("Неверный формат файла");
            _header = ReadHeader(); // Копируем заголовок из файла в _header
        }

        /// <summary>
        /// Cоздает файл по указанному пути
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        private void Create(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Extension != ".dbt") throw new ArgumentException("Неверный формат файла");
            _header = new DbtHeader();
            _stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            RewriteFile();
        }

        /// <summary>
        /// Читает информацию из заголовка .dbt файла
        /// </summary>
        /// <returns></returns>
        DbtHeader ReadHeader()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            byte[] buf = new byte[Constants.headerSize];
            _stream.Read(buf, 0, Constants.headerSize);
            uint nextFreeBlock = Convert.ToUInt32(Encoding.ASCII.GetString(buf));
            return new DbtHeader(nextFreeBlock);
        }

        /// <summary>
        /// Полностью перезаписывает весь файл с учётом новых данных
        /// </summary>
        /// <param name="header">Заголовок файла</param>
        /// <param name="data">Текст для записи в файл</param>
        private void RewriteFile(byte[] data = null)
        {
            if (data != null)
            {
                RewriteHeader(data);
                int lastLen = data.Length % Constants.blockSize, len = data.Length / Constants.blockSize + ((lastLen > 0) ? 1 : 0);
                _stream.Seek(Constants.blockSize, SeekOrigin.Begin);
                for (int i = 0; i < len; i++) // Запись по блокам в файл
                {
                    var buf = new byte[Constants.blockSize];
                    Buffer.BlockCopy(data, i * Constants.blockSize, buf, 0, Constants.blockSize);
                    _stream.Write(buf, 0, Constants.blockSize);
                }
            }
            else RewriteHeader();
        }

        /// <summary>
        /// Меняет заголовок файла. Если подать ещё data, то поменяет заголовок с учетом новой информации
        /// </summary>
        /// <param name="data">Новая информация</param>
        private void RewriteHeader(byte[] data = null)
        {
            if (data != null) _header.ChangeHeader(data);
            _stream.Seek(0, SeekOrigin.Begin);
            byte[] header = new byte[Constants.blockSize], head = Encoding.ASCII.GetBytes(_header.NextFreeBlock.ToString());
            Buffer.BlockCopy(head, 0, header, 0, head.Length);
            _stream.Write(header, 0, Constants.blockSize);
        }

        /// <summary>
        /// Добавляет в конец файла новый блок информации
        /// </summary>
        /// <param name="data">Текст, который вы хотите записать в конец файла</param>
        public void AddData(byte[] data)
        {
            if (data.Length > Constants.blockSize) throw new ArgumentException($"Текст не может быть больше {Constants.blockSize} байт");
            var buf = new byte[Constants.blockSize];
            Buffer.BlockCopy(data, 0, buf, 0, data.Length);
            _stream.Seek((_header.NextFreeBlock - 1) * Constants.blockSize, SeekOrigin.Begin);
            _stream.Write(buf, 0, Constants.blockSize);
            RewriteHeader(data);
        }    

        /// <summary>
        /// Возвращает текст блока по номеру (нумерация с 1)
        /// </summary>
        /// <param name="number"></param>
        /// <returns>Текст данного блока</returns>
        public byte[] GetBlockData(uint number)
        {
            if (number == 1) throw new ArgumentException("Нельзя получить текст с заголовока .dbt файла с помощью этого метода");
            if (number >= _header.NextFreeBlock) throw new ArgumentOutOfRangeException("Блока с таким номером не существует");
            _stream.Seek((number - 1) * Constants.blockSize, SeekOrigin.Begin);
            var buf = new byte[Constants.blockSize];
            _stream.Read(buf, 0, Constants.blockSize);
            return buf;
        }
        
        /// <summary>
        /// Закрывает поток
        /// </summary>
        public void Close()
        {
            _stream.Close();
        }
    }
}
