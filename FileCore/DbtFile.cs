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
        /// Конструктор для открытия существуещего файла
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        public DbtFile(string path)
        {
            Open(path);
        }
        /// <summary>
        /// Для создания файла по уже существуещему заголовку, при header = null создаться файл с заголовком без других блоков текста
        /// </summary>
        /// <param name="path">Путь до файла</param>
        /// <param name="header">Заголовок файла, при header = null создаться файл с заголовком без других блоков текста</param>
        public DbtFile(string path, DbtHeader header)
        {
            Create(path, header);
        }

        /// <summary>
        /// Открывает файл по указанному пути
        /// </summary>
        /// <param name="path">Путь до файла</param>
        void Open(string path)
        {
            _stream = new FileStream(path, FileMode.Open);
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Extension != ".dbt") throw new ArgumentException("Wrong file extension");
            _header = ReadFile(); // Копируем информацию из файла в _header
        }

        /// <summary>
        /// Читает всю информацию из .dbt файла
        /// </summary>
        /// <returns></returns>
        DbtHeader ReadFile()
        {
            _stream.Seek(_header.BlockSize - _header.HeaderSize, SeekOrigin.Begin);
            byte[] buf = new byte[_header.HeaderSize];
            _stream.Read(buf, 0, _header.HeaderSize);
            uint nextFreeBlock = BitConverter.ToUInt32(buf, 0);
            _stream.Seek(_header.BlockSize, SeekOrigin.Begin);
            byte[] data = new byte[(nextFreeBlock - 1) * _header.BlockSize];
            _stream.Read(data, 0, data.Length);
            return new DbtHeader(data);
        }

        /// <summary>
        /// Cоздает файл по указанному пути
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <param name="header">Заголовок файла, при header = null создаться файл с заголовком без других блоков текста</param>
        /// <exception cref="ArgumentException">Неправильный формат файла</exception>
        private void Create(string path, DbtHeader header = null)
        {
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Extension != ".dbt") throw new ArgumentException("Wrong file extension");
            if (header == null) _header = new DbtHeader();
            else _header = new DbtHeader(header.GetByte());
            _stream = new FileStream(path, FileMode.Create);
            RewriteFile(_header);
        }

        /// <summary>
        /// Полностью перезаписывает весь файл с учётом нового DbtHeader
        /// </summary>
        /// <param name="header">Заголовок файла</param>
        public void RewriteFile(DbtHeader header)
        {
            _stream.Seek(0, SeekOrigin.Begin);
            byte[] buf = new byte[header.BlockSize];
            Buffer.BlockCopy(BitConverter.GetBytes(header.NextFreeBlock), 0, buf, 0, 3);
            _stream.Write(buf, 0, header.BlockSize);
            _stream.Seek(header.BlockSize, SeekOrigin.Begin);
            var head = header.GetByte();
            _stream.Write(head, 0, head.Length);
        }

        /// <summary>
        /// Изменяет блок .dbt файла по номеру
        /// </summary>
        /// <param name="data">Текст, который вы хотите записать в этот блок</param>
        /// <param name="number">Номер нужного блока (нумерация начинается с 1)</param>
        public void UpdateBlock(byte[] data, uint number) => _header.UpdateBlock(data, number);

        /// <summary>
        /// Добавляет в конец файла новую информацию
        /// </summary>
        /// <param name="data">Текст, который вы хотите записать в конец файла</param>
        /// <exception cref="ArgumentException"></exception>
        public void AddData(byte[] data)
        {
            if (data.Length > _header.BlockSize) throw new ArgumentException($"Текст не может быть больше {_header.BlockSize} байт");
            _header.AddData(data);
        }

        /// <summary>
        /// Удаляет блок по номеру
        /// </summary>
        /// <param name="number">Номер нужного блока (нумерация начинается с 1)</param>
        public void RemoveBlock(uint number) => _header.RemoveBlock(number);
        

        /// <summary>
        /// Возвращает текст блока по номеру (нумерация с 1)
        /// </summary>
        /// <param name="number"></param>
        /// <returns>Текст данного блока</returns>
        public byte[] GetBlockData(uint number) => _header.GetBlockData(number);
        

        /// <summary>
        /// Закрывает поток, сохраняя данные в файл
        /// </summary>
        public void Close()
        {
            RewriteFile(_header);
            _stream.Close();
        }
    }
}
