using System;
using System.Collections.Generic;

namespace SQLInterpreter.Properties.FileCore
{
    public class DbfHeader
    {
        private short _minHeader = 33;
        private short _fieldSize = 32;
        private byte _hasMemo = Constants.NoMemo;
        private byte[] _date = new byte[3];
        private byte[] _count = new byte[4];
        private byte[] _headerSize = new byte[2];
        private byte[] _entrySize = new byte[2];
        private byte[] _reserved = new byte[20];
        private List<DbfField> _fields = new List<DbfField>();
        private byte _terminator = Constants.Terminator;

        public DbfHeader(bool hasNemo, DateTime date, int count, short headerSize, short entrySize)
        {
            HasMemo = hasNemo;
            Date = date;
            Count = count;
            HeaderSize = headerSize;
            EntrySize = entrySize;
        }

        public DbfHeader(byte[] data)
        {
            if (data.Length < _minHeader || (data.Length - _minHeader) % _fieldSize != 0)
                throw new ArgumentException("Cannot convert data to dbf header");
            _hasMemo = data[0];
            Buffer.BlockCopy(data, 1, _date, 0, _date.Length);
            Buffer.BlockCopy(data, 4, _count, 0, _count.Length);
            Buffer.BlockCopy(data, 8, _headerSize, 0, _headerSize.Length);
            Buffer.BlockCopy(data, 10, _entrySize, 0, _entrySize.Length);
            int offset = 32;
            for (int i = 1; i <= (data.Length - _minHeader) / _fieldSize; i++)
            {
                var buf = new byte[_fieldSize];
                Buffer.BlockCopy(data, i*offset, buf, 0, buf.Length);
                _fields.Add(new DbfField(buf));
            }


        }
        
        public bool HasMemo
        {
            get => (_hasMemo==Constants.Memo);
            set
            {
                _hasMemo = (value)?Constants.Memo:Constants.NoMemo;
            }
        }

        public DateTime Date
        {
            get => new DateTime(_date[0], _date[1],_date[2]);
            set
            {
                _date[0] = (byte)value.Year;
                _date[1] = (byte)value.Month;
                _date[2] = (byte)value.Day;
            }
        }

        public int Count
        {
            get=>BitConverter.ToInt32(_count, 0);
            set{
                if (value < 0) throw new ArgumentException("Count cannot be les then zero");
                var v = BitConverter.GetBytes(value);
                Buffer.BlockCopy(v,0,_count,0,v.Length);
            }
        }

        public short HeaderSize
        {
            get=>BitConverter.ToInt16(_headerSize, 0);
            set
            {
                if (value < 0) throw new ArgumentException("header size cannot be les then zero");
                var v = BitConverter.GetBytes(value);
                Buffer.BlockCopy(v,0,_headerSize,0,v.Length);
            }
        }
        public short EntrySize
        {
            get=>BitConverter.ToInt16(_entrySize, 0);
            set
            {
                if (value < 0) throw new ArgumentException("header size cannot be les then zero");
                var v = BitConverter.GetBytes(value);
                Buffer.BlockCopy(v,0,_entrySize,0,v.Length);
            }
        }

        public List<DbfField> Fields
        {
            get => _fields;
            set => _fields = value;
        }

        public byte[] GetByte()
        {
            var buf = new byte[_minHeader+_fieldSize*_fields.Count];
            buf[0] = _hasMemo;
            Buffer.BlockCopy(_date,0,buf,1,_date.Length);
            Buffer.BlockCopy(_count,0,buf,4,_count.Length);
            Buffer.BlockCopy(_headerSize,0,buf,8,_headerSize.Length);
            Buffer.BlockCopy(_entrySize,0,buf,10,_entrySize.Length);
            Buffer.BlockCopy(_reserved,0,buf,12,_reserved.Length);
            int offset = 32;
            foreach (var i in _fields)
            {
                var b = i.GetByte();
                Buffer.BlockCopy(b,0,buf,offset,b.Length);
                offset += _fieldSize;
            }
            buf[offset] = _terminator;
            return buf;
        }
    }
}