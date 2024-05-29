using System;
using System.Collections;
using System.Collections.Generic;

namespace SQLInterpreter.Properties.FileCore
{
    /// <summary>
    /// для работы с записью
    /// </summary>
    public class Entry
    {
        private DbfHeader _header;
        private byte[] _entry;

        private static IEnumerator<DbfField> GetField(List<DbfField> fields)
        {
            foreach (var i in fields)
            {
                yield return i;
            }

            while (true) yield return null;
            yield break;
        } 
        /// <summary>
        /// для перевода записи из одного формата в другой
        /// </summary>
        /// <param name="oldEntry">переводимая запись</param>
        /// <param name="newEntryTemplate">новый формат записи, на основе заголовка</param>
        /// <returns></returns>
        public static Entry ConvertEntry(Entry oldEntry, DbfHeader newEntryTemplate)
        {
            var generator = GetField(oldEntry.Header.Fields);
            generator.MoveNext();
            var entry = new Entry(newEntryTemplate);
            entry.IsDeleted = oldEntry.IsDeleted;
            foreach (var i in newEntryTemplate.Fields)
            {
                while (generator.Current != null && generator.Current.Name != i.Name) generator.MoveNext();
                if (generator.Current != null)
                {
                    var f = generator.Current;
                    var buf = new byte[i.Size]; 
                    Buffer.BlockCopy(buf,0,entry.GetByte(),i.Offset,i.Size);
                    byte size = (i.Size < f.Size) ? i.Size : f.Size;
                    Buffer.BlockCopy(oldEntry.GetByte(),f.Offset,entry.GetByte(),i.Offset,size);
                }
            }
            return entry;
        }
        
        public static Entry UpdateEntry(Entry oldEntry, DbfHeader newEntryTemplate)//здесь ваша система,
                                                                                   //учитывающая приведения типов
        {
            var generator = GetField(oldEntry.Header.Fields);
            generator.MoveNext();
            var entry = new Entry(newEntryTemplate);
            entry.IsDeleted = oldEntry.IsDeleted;
            foreach (var i in newEntryTemplate.Fields)
            {
                while (generator.Current != null && generator.Current.Name != i.Name) generator.MoveNext();
                if (generator.Current != null)
                {
                    var f = generator.Current;
                    var buf = new byte[i.Size]; 
                    Buffer.BlockCopy(buf,0,entry.GetByte(),i.Offset,i.Size);
                    byte size = (i.Size < f.Size) ? i.Size : f.Size;
                    Buffer.BlockCopy(oldEntry.GetByte(),f.Offset,entry.GetByte(),i.Offset,size);
                }
            }
            return entry;
        }
    
        public Entry(DbfHeader header)
        {
            _header = new DbfHeader(header.GetByte());
            _entry = new byte[header.EntrySize];
            _entry[0] = Constants.NoDelete;
        }
        
        public Entry(DbfHeader header, byte[] entry)
        {
            _header = new DbfHeader(header.GetByte());
            _entry = entry;
        }

        public DbfHeader Header
        {
            get => _header;
            set => _header = value;
        }

        public bool IsDeleted
        {
            get => (_entry[0] == Constants.Delete);
            set => _entry[0] = (value) ? Constants.Delete : Constants.NoDelete;
        }
        /// <summary>
        /// для получения значения записи по имени поля
        /// </summary>
        /// <param name="fieldName">имя получаемого поля</param>
        /// <returns>возвращает данные в виде массива байтов</returns>
        /// <exception cref="ArgumentException"></exception>
        public byte[] GetValue(string fieldName)
        {
            foreach (var i in _header.Fields)
            {
                if (i.Name == fieldName)
                {
                    int offset = i.Offset;
                    short size = i.Size;
                    var buf = new byte [size];
                    Buffer.BlockCopy(_entry,offset,buf,0,size);
                    return buf;
                }
            }
            throw new ArgumentException("field with this name had not found");
        }
        /// <summary>
        /// для обновления значения в записи по имени поля
        /// </summary>
        /// <param name="fieldName">имя поля</param>
        /// <param name="data">данные для записи</param>
        /// <exception cref="ArgumentException"></exception>
        public void Update(string fieldName, byte[] data)
        {
            foreach (var i in _header.Fields)
            {
                if (i.Name == fieldName)
                {
                    int offset = i.Offset;
                    short size = i.Size;
                    if (data.Length > size) throw new ArgumentException("data is too big");
                    var buf = new byte [size];
                    Buffer.BlockCopy(buf,0,_entry,offset,size);
                    Buffer.BlockCopy(data,0,_entry,offset,data.Length);
                    return;
                }
            }
            throw new ArgumentException("field with this name had not found");
        }
        /// <summary>
        /// для получения записи в виде массива байт
        /// </summary>
        /// <returns></returns>
        public byte[] GetByte()
        {
            return _entry;
        }
    }
}