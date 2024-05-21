using System;
using System.Text;

namespace SQLInterpreter.Properties.FileCore
{
    /// <summary>
    /// Класс для описания поля таблицы
    /// </summary>
    public class DbfField
    {
        private short _fieldSize = 32;
        private ASCIIEncoding _encoding = new ASCIIEncoding(); 
        private byte[] _name = new byte[11];
        private byte _type;
        private byte[] _offset = new byte[4];
        private byte _size;
        private byte _accuracy;
        private byte[] _reserved = new byte[14];

        public DbfField(string name, char type, int offset, byte size, byte accuracy)
        {
            Name = name;
            Type = type;
            Offset = offset;
            Size = _size;
            Accuracy = accuracy;
        }

        public DbfField(byte[] data)
        {
            if (data.Length != _fieldSize) throw new ArgumentException("Cannot convert data to field");
            
            Buffer.BlockCopy(data,0,_name,0,_name.Length);
            _type = data[11];
            Buffer.BlockCopy(data,12,_offset,0,_offset.Length);
            _size = data[16];
            _accuracy = data[17];
        }
        public String Name
        {
            get => _encoding.GetString(_name,0,_name.Length);
            set
            {
                if (value.Length > 11) throw new ArgumentException("name is too big");
                var v = Encoding.ASCII.GetBytes(value);
                for (int i =0;i<_name.Length;i++)
                {
                    _name[i] = 0;
                }
                Buffer.BlockCopy(v,0,_name,0,v.Length);
            }
        }

        public char Type
        {
            get => (char)_type;
            set
            {
                if (Constants.IsCorrectType(value)) _type = (byte)value;
                else throw new ArgumentException("incorrect data type");
            }
        }

        public int Offset
        {
            get => BitConverter.ToInt32(_offset, 0);
            set
            {
                if (value < 0) throw new ArgumentException("offset cannot be les then zero");
                var v = BitConverter.GetBytes(value);
                Buffer.BlockCopy(v,0,_offset,0,v.Length);   
            }
        }

        public byte Size
        {
            get => _size;
            set => _size = value;
        }

        public byte Accuracy
        {
            get => _accuracy;
            set => _accuracy = value;
        }

        public byte[] GetByte()
        {
            byte[] v = new byte [_fieldSize];
            Buffer.BlockCopy(_name,0,v,0,_name.Length);
            v[11] = _type;
            Buffer.BlockCopy(_offset,0,v,12,_offset.Length);
            v[16] = _size;
            v[17] = _accuracy;
            Buffer.BlockCopy(_reserved,0,v,18,_reserved.Length);
            return v;
        }
    }
}