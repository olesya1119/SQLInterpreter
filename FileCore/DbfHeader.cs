using System;
using System.Collections.Generic;

namespace SQLInterpreter.Properties.FileCore
{
    /// <summary>
    /// для хранения заголовка
    /// </summary>
    public class DbfHeader
    {
        private short _minHeader = 33;
        private short _fieldSize = 32;
        private byte _hasMemo = Constants.NoMemo; //наличие мемо
        private byte[] _date = new byte[3];//дата
        private byte[] _count = new byte[4];//количество записей
        private byte[] _headerSize = new byte[2];//размер заголовка
        private byte[] _entrySize = new byte[2];//размер записи
        private byte[] _reserved = new byte[20];//все зарезервированные 
        private List<DbfField> _fields = new List<DbfField>();//список полей
        private byte _terminator = Constants.Terminator;//признак окончания заголовка

        public DbfHeader()
        {
            HasMemo = false;
            Date = DateTime.Now;
            Count = 0;
            HeaderSize = (short)_minHeader;
            EntrySize = (short)1;
        }

        public DbfHeader(bool hasMemo, DateTime date, int count, short headerSize, short entrySize, List<DbfField> fields)
        {
            HasMemo = hasMemo;
            Date = date;
            Count = count;
            HeaderSize = headerSize;
            EntrySize = entrySize;
            foreach (var i in fields)
            {
                var f = new DbfField(i.Name, i.Type, i.Offset, i.Size, i.Accuracy);
                _fields.Add(f);
            }
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
                Buffer.BlockCopy(data, i * offset, buf, 0, buf.Length);
                _fields.Add(new DbfField(buf));
            }
        }
        /// <summary>
        /// добавляет поле в заголовок
        /// </summary>
        /// <param name="field">добавляемое поле</param>
        /// <exception cref="ArgumentException"></exception>
        public void AddField(DbfField field)
        {
            DbfField f = (DbfField)field.Clone();
            int offset = 1;
            foreach (var i in _fields)
            {
                if (i.Name == field.Name) throw new ArgumentException("Поле с таким именем уже существует");
                offset += i.Size;
            }
            if (field.Type == 'M') HasMemo = true;
            _fields.Add(f);
            Date = DateTime.Now;
            HeaderSize += _fieldSize;
            EntrySize += f.Size;
            f.Offset = offset;
        }
        /// <summary>
        /// переименовывает поле в заголовки
        /// </summary>
        /// <param name="oldName">старое имя поля</param>
        /// <param name="newName">новое имя поля</param>
        /// <exception cref="ArgumentException"></exception>
        public void RenameField(string oldName, string newName)
        {
            foreach (var i in _fields)
            {
                if (i.Name == oldName)
                {
                    i.Name = newName;
                    return;
                }
            }
            throw new ArgumentException("Поле с таким именем не найдено");
        }
        /// <summary>
        /// обновляет поле в заголовки
        /// </summary>
        /// <param name="field">поле</param>
        /// <exception cref="ArgumentException"></exception>
        public void UpdateField(DbfField field)
        {
            bool isFound = false;
            for (int i = 0; i < _fields.Count; i++)
            {
                if (_fields[i].Name == field.Name)
                {
                    _fields[i] = field;
                    isFound = true;
                    break;
                }
            }
            if (!isFound) throw new ArgumentException("Поле с таким именем не найдено");
            bool hasMemo = false;
            int offset = 1;
            short entrySize = 1;
            foreach (var i in _fields)
            {
                if (i.Type == 'M') hasMemo = true;
                i.Offset = offset;
                entrySize += i.Size;
                offset += i.Size;
            }
            EntrySize = entrySize;
            HasMemo = hasMemo;
            Date = DateTime.Now;

        }
        /// <summary>
        /// удаляет поле из заголовка по его имени
        /// </summary>
        /// <param name="fieldName">имя поля</param>
        /// <exception cref="ArgumentException"></exception>
        public void RemoveField(string fieldName)
        {
            bool isFound = false;
            short deleteEntrySize = 0;
            foreach (var i in _fields)
            {
                if (i.Name == fieldName)
                {
                    isFound = true;
                    deleteEntrySize = i.Size;
                    _fields.Remove(i);
                    break;
                }
            }
            if (!isFound) throw new ArgumentException("Поле с таким именем не найдено");
            bool hasMemo = false;
            int offset = 1;
            foreach (var i in _fields)
            {
                if (i.Type == 'M') hasMemo = true;
                i.Offset = offset;
                offset += i.Size;
            }
            HasMemo = hasMemo;
            Date = DateTime.Now;
            HeaderSize -= _fieldSize;
            EntrySize -= deleteEntrySize;
        }

        /// <summary>
        /// Получить поле из заголовка по имени поля
        /// </summary>
        /// <param name="fieldName">Имя поля</param>
        /// <returns></returns>
        public DbfField GetField(string fieldName)
        {
            foreach (var v in _fields)
            {
                if (v.Name == fieldName) return v;
            }
            throw new ArgumentException("Поля с таким именем не было найдено");
        }

        public bool HasMemo
        {
            get => (_hasMemo == Constants.Memo);
            set
            {
                _hasMemo = (value) ? Constants.Memo : Constants.NoMemo;
            }
        }

        public DateTime Date
        {
            get => new DateTime(2000 + _date[0], _date[1], _date[2]);
            set
            {
                _date[0] = (byte)(value.Year % 100);
                _date[1] = (byte)value.Month;
                _date[2] = (byte)value.Day;
            }
        }

        public int Count
        {
            get => BitConverter.ToInt32(_count, 0);
            set
            {
                if (value < 0) throw new ArgumentException("Count cannot be les then zero");
                var v = BitConverter.GetBytes(value);
                Buffer.BlockCopy(v, 0, _count, 0, v.Length);
            }
        }

        public short HeaderSize
        {
            get => BitConverter.ToInt16(_headerSize, 0);
            set
            {
                if (value < 0) throw new ArgumentException("header size cannot be les then zero");
                var v = BitConverter.GetBytes(value);
                Buffer.BlockCopy(v, 0, _headerSize, 0, v.Length);
            }
        }
        public short EntrySize
        {
            get => BitConverter.ToInt16(_entrySize, 0);
            set
            {
                if (value < 0) throw new ArgumentException("header size cannot be les then zero");
                var v = BitConverter.GetBytes(value);
                Buffer.BlockCopy(v, 0, _entrySize, 0, v.Length);
            }
        }

        public List<DbfField> Fields
        {
            get => _fields;
            set => _fields = value;
        }

        public byte[] GetByte()
        {
            var buf = new byte[_minHeader + _fieldSize * _fields.Count];
            buf[0] = _hasMemo;
            Buffer.BlockCopy(_date, 0, buf, 1, _date.Length);
            Buffer.BlockCopy(_count, 0, buf, 4, _count.Length);
            Buffer.BlockCopy(_headerSize, 0, buf, 8, _headerSize.Length);
            Buffer.BlockCopy(_entrySize, 0, buf, 10, _entrySize.Length);
            Buffer.BlockCopy(_reserved, 0, buf, 12, _reserved.Length);
            int offset = 32;
            foreach (var i in _fields)
            {
                var b = i.GetByte();
                Buffer.BlockCopy(b, 0, buf, offset, b.Length);
                offset += _fieldSize;
            }
            buf[offset] = _terminator;
            return buf;
        }
    }
}