using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace SQLInterpreter.Properties.FileCore
{
    /// <summary>
    /// для работы с данными находящимися  вфайле
    /// </summary>
    public class EntryVirtualArray
    {
        private FileStream _stream;
        private DbfHeader _header;

        public Entry this[int index]
        {
            get => ReadEntry(index);
            set => WriteEntry(index,value);
        }

        public DbfHeader Header => _header;
        /// <summary>
        ///конструктор для открытия существуещего файла
        /// </summary>
        /// <param name="path">путь к файлу</param>
        public EntryVirtualArray(string path)
        {
            Open(path);
        }
        /// <summary>
        /// для создания файла по уже существуещему заголовку, при header=null создаться файл с заголовком без полей
        /// </summary>
        /// <param name="path">путь до файла</param>
        /// <param name="header">заголовок файла, при header=null создаться файл с заголовком без полей </param>
        public EntryVirtualArray(string path, DbfHeader header)
        {
            Create(path,header);
        }
        /// <summary>
        /// открывает файл по указанному пути
        /// </summary>
        /// <param name="path">путь до файла</param>
        private void Open(string path)
        {
            _stream = new FileStream(path, FileMode.Open);
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Extension != ".dbf") throw new ArgumentException("Wrong file extension");
            _header = ReadHeader();
        }
        /// <summary>
        /// создает файл по указанному пути
        /// </summary>
        /// <param name="path">путь к файлу</param>
        /// <param name="header">заголовок файла, при header=null создаться файл с заголовком без полей</param>
        /// <exception cref="ArgumentException"></exception>
        private void Create(string path, DbfHeader header)
        {
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Extension != ".dbf") throw new ArgumentException("Wrong file extension");
            if (header == null) _header = new DbfHeader();
            else _header = new DbfHeader(header.GetByte());
            _stream = new FileStream(path, FileMode.Create);
            WriteHeader();
        }
        /// <summary>
        /// закрывает общение с файлом
        /// </summary>
        public void Close()
        {
            WriteHeader();
            _stream.Close();
        }
        /// <summary>
        /// читает заголовок из файла
        /// </summary>
        /// <returns></returns>
        private DbfHeader ReadHeader()
        {
            _stream.Seek(8, SeekOrigin.Begin);
            byte[] buf = new byte[2];
            _stream.Read(buf, 0, 2);
            short headerSize = BitConverter.ToInt16(buf, 0);
            buf = new byte[headerSize];
            _stream.Seek(0, SeekOrigin.Begin);
            _stream.Read(buf, 0, headerSize);
            return new DbfHeader(buf);
        }
        /// <summary>
        /// записывает заголовок  в файл
        /// </summary>
        public void WriteHeader()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            var header = _header.GetByte();
            _stream.Write(header,0,header.Length);
        }
        /// <summary>
        /// записывает запись в файл по заданному индексу
        /// </summary>
        /// <param name="index">индекс записи</param>
        /// <param name="entry">запись</param>
        public void WriteEntry(int index, Entry entry)
        {
            long offset = FindOffset(index);
            _stream.Seek(offset, SeekOrigin.Begin);
            var e = entry.GetByte();
            _stream.Write(e, 0, e.Length);
        }
        /// <summary>
        /// читает запись из файла по заданному индексу
        /// </summary>
        /// <param name="index">индекс записи</param>
        /// <param name="entry">запись</param>
        public Entry ReadEntry(int index)
        {
            long offset = FindOffset(index);
            _stream.Seek(offset, SeekOrigin.Begin);
            var e = new byte[_header.EntrySize];
            _stream.Read(e, 0, e.Length);
            return new Entry(_header, e);
        }

        /// <summary>
        /// читает одно из полей записи из файла по заданному индексу и имении поля
        /// </summary>
        /// <param name="index">индекс записи</param>
        /// <param name="entry">запись</param>
        /// <param name="fieldName">имя читаемого поля</param>
        public byte[] ReadFieldFromEntry(int index, string fieldName)
        {
            foreach (var i in _header.Fields)
            {
                if (i.Name == fieldName)
                {
                    long offset = FindOffset(index)+i.Offset;
                    _stream.Seek(offset, SeekOrigin.Begin);
                    var buf = new byte[i.Size];
                    _stream.Read(buf, 0, buf.Length);
                    return buf;
                }
            }
            throw new ArgumentException("field with this name had not found");
        }
        /// <summary>
        /// Меняет пометку об удаление записи
        /// </summary>
        /// <param name="index">индекс записи</param>
        /// <param name="entryStatus">статус заявки</param>
        /// <exception cref="ArgumentException"></exception>
        public void SetEntryStatus(int index, byte entryStatus)
        {
            long offset = FindOffset(index);
            _stream.Seek(offset, SeekOrigin.Begin);
            if (entryStatus == Constants.Delete || entryStatus == Constants.NoDelete)
            {
                var buf = new byte[1];
                buf[0] = entryStatus;
                _stream.Write(buf,0,buf.Length);
            }
            else throw new ArgumentException("Wrong entry status");
        }
        /// <summary>
        /// Получает пометку об удаление записи
        /// </summary>
        /// <param name="index">индекс записи</param>
        /// <exception cref="ArgumentException"></exception>
        public byte[] GetEntryStatus(int index)
        {
            long offset = FindOffset(index);
            _stream.Seek(offset, SeekOrigin.Begin);
            var buf = new byte[1];
            _stream.Read(buf, 0, buf.Length);
            return buf;
        }

        private long FindOffset(int index)
        {
            if (index < 0 || index >= _header.Count) throw new IndexOutOfRangeException("index bigger then max");
            return _header.HeaderSize + index * _header.EntrySize;
        }
        /// <summary>
        /// для удаления ненужных данных из конца файла
        /// </summary>
        /// <param name="index">индекс записи перед которой происходит удаление</param>
        public void WriteEnd(int index)
        {
            long offset = FindOffset(index);
            _stream.SetLength(offset);
        }
        /// <summary>
        /// добавляет запись в конец файла
        /// </summary>
        /// <param name="entry">запись</param>
        public void AppendEntry(Entry entry)
        {
            long offset = (_header.Count!=0)?FindOffset(_header.Count-1)+_header.EntrySize:_header.HeaderSize;
            _stream.Seek(offset, SeekOrigin.Begin);
            var e = entry.GetByte();
            _stream.Write(e,0,e.Length);
            _header.Count += 1;
            _header.Date = DateTime.Now;
            WriteHeader();
        }
    }
}