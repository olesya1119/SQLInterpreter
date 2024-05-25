using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace SQLInterpreter.Properties.FileCore
{
    public class EntryVirtualArray
    {
        private FileStream _stream;
        private DbfHeader _header;

        public Entry this[int index]
        {
            get => ReadEntry(index);
            set => WriteEntry(value, index);
        }

        public DbfHeader Header => _header;

        public EntryVirtualArray(string path)
        {
            Open(path);
        }

        public EntryVirtualArray(string path, DbfHeader header)
        {
            Create(path,header);
        }

        private void Open(string path)
        {
            _stream = new FileStream(path, FileMode.Open);
            _header = ReadHeader();
        }

        private void Create(string path, DbfHeader header)
        {
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Extension != ".dbf") throw new ArgumentException("Wrong file extension");
            if (header == null) _header = new DbfHeader();
            else _header = new DbfHeader(header.GetByte());
            _stream = new FileStream(path, FileMode.Create);
            WriteHeader();
        }

        public void Close()
        {
            _stream.Close();
        }

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
        
        public void WriteHeader()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            var header = _header.GetByte();
            _stream.Write(header,0,header.Length);
        }

        private void WriteEntry(Entry entry, int index)
        {
            long offset = FindOffset(index);
            _stream.Seek(offset, SeekOrigin.Begin);
            var e = entry.GetByte();
            _stream.Write(e, 0, e.Length);
        }

        private Entry ReadEntry(int index)
        {
            long offset = FindOffset(index);
            _stream.Seek(offset, SeekOrigin.Begin);
            var e = new byte[_header.EntrySize];
            _stream.Read(e, 0, e.Length);
            return new Entry(_header, e);
        }

        private long FindOffset(int index)
        {
            if (index < 0 || index >= _header.Count) throw new IndexOutOfRangeException("index bigger then max");
            return _header.HeaderSize + index * _header.EntrySize;
        }
        
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