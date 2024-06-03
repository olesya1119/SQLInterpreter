using SQLInterpreter.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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


        private static void CastFields(DbfField oldField, DbfField newField, Entry oldData, Entry newData)
        {
            if (oldField.Type == 'C' && newField.Type == 'N') // Строка -> числовой тип 
            {
                bool isNotDigitFlag = false;
               
                byte size = (newField.Size < oldField.Size) ? newField.Size : oldField.Size;
                string data = Encoding.ASCII.GetString(oldData.GetValue(oldField.Name));
                foreach (char c in data)
                {
                    if (!char.IsDigit(c) && c!='.' && c != '\0') isNotDigitFlag = true;
                }
                if (isNotDigitFlag)//записываем пустую строку если привидение невозможно
                {
                    data = "";
                    for (int i = 0; i < size; i++) data += '\0';
                    Buffer.BlockCopy(Encoding.ASCII.GetBytes(data), 0, newData.GetByte(), newField.Offset, size);

                }
                else
                    data=NumberStringCheck.FormatString(data, size, newField.Accuracy);
                    Buffer.BlockCopy(Encoding.ASCII.GetBytes(data), 0, newData.GetByte(), newField.Offset, size);
            }
            else
            if (oldField.Type == 'L' && newField.Type == 'N' && newField.Accuracy == 0)//boolean => Целое
            {
                byte size = (newField.Size < oldField.Size) ? newField.Size : oldField.Size;
                //byte size = (newField.Size < oldField.Size) ? oldField.Size : newField.Size;
                //Buffer.BlockCopy(oldData.GetByte(), oldField.Offset, newData.GetByte(), newField.Offset, size);

                string data = Encoding.ASCII.GetString(oldData.GetValue(oldField.Name));
                if (data.Equals("t")) data = "1";
                if (data.Equals("f")) data = "0";
                if (data.Equals("n")) data = "0";

                Buffer.BlockCopy(new byte[] { (byte)data[0] }, 0, newData.GetByte(), newField.Offset, size);


            }else
            if (oldField.Type == 'N' && oldField.Accuracy > 0 && newField.Type == 'N' && newField.Accuracy == 0)//Вещественное – целое с потерей дробной части
            {
                byte size = (newField.Size < oldField.Size) ? newField.Size : oldField.Size;
                string data = Encoding.ASCII.GetString(oldData.GetValue(oldField.Name));
                data = data.Remove(data.IndexOf('.'), oldField.Accuracy+1);
                data = data.PadRight(size, '\0');
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(data), 0, newData.GetByte(), newField.Offset, size);
            }
            else
            if(oldField.Type == 'N' && oldField.Accuracy==0 && newField.Type == 'N' && newField.Accuracy > 0) //Целое -> вещественное с дробной частью, равной 0
            {
                byte size = (newField.Size < oldField.Size) ? newField.Size : oldField.Size;
                string data = Encoding.ASCII.GetString(oldData.GetValue(oldField.Name));
                data=data.Insert(data.IndexOf('\0'),".");
                data = data.Remove(data.Length-1);
                for (int i = 0; i < newField.Accuracy; i++)
                {
                    data=data.Insert(data.IndexOf('\0'), "0");
                    data = data.Remove(data.Length-1);
                    
                    if (data.IndexOf('\0') == size-1) break;
                }
               
                Buffer.BlockCopy( Encoding.ASCII.GetBytes(data), 0, newData.GetByte(), newField.Offset, size);
            }else
            if(oldField.Type == 'N' && newField.Type == 'L') // Целое/вещественное => boolean
            {
                byte size = (newField.Size < oldField.Size) ? newField.Size : oldField.Size;

                string data = Encoding.ASCII.GetString(oldData.GetValue(oldField.Name));
                if (int.Parse(data) >= 0) data = "t";
                else data = "f";

                Buffer.BlockCopy(new byte[] { (byte)data[0] }, 0, newData.GetByte(), newField.Offset, size);
            }
            else if(oldField.Type == 'N' && newField.Type == 'C')
            {
                byte size = (newField.Size < oldField.Size) ? newField.Size : oldField.Size;
                Buffer.BlockCopy(oldData.GetByte(), oldField.Offset, newData.GetByte(), newField.Offset, size);
            }
            else if (oldField.Type == newField.Type && oldField.Accuracy == newField.Accuracy) // нет привидения типа
            {
                byte size = (newField.Size < oldField.Size) ? newField.Size : oldField.Size;
                Buffer.BlockCopy(oldData.GetByte(), oldField.Offset, newData.GetByte(), newField.Offset, size);
            }

            else//неправильное привидение типа
            {
                 byte size = (newField.Size < oldField.Size) ? newField.Size : oldField.Size;
                 string data = new string('\0', size);
                 Buffer.BlockCopy(Encoding.ASCII.GetBytes(data), 0, newData.GetByte(), newField.Offset, size);
            }
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
                    var f = generator.Current;//старое поле
                    var buf = new byte[i.Size];
                    Buffer.BlockCopy(buf, 0, entry.GetByte(), i.Offset, i.Size);//заполняем буффер нулями
                    // i и f - новое и старое поле
                    CastFields(f, i, oldEntry, entry);
                    //byte size = (i.Size < f.Size) ? i.Size : f.Size;
                    //Buffer.BlockCopy(oldEntry.GetByte(), f.Offset, entry.GetByte(), i.Offset, size);// копирование из старого поля в новое

                    //var f = generator.Current;
                    //var buf = new byte[i.Size];
                    //Buffer.BlockCopy(buf, 0, entry.GetByte(), i.Offset, i.Size);
                    //byte size = (i.Size < f.Size) ? i.Size : f.Size;
                    //Buffer.BlockCopy(oldEntry.GetByte(), f.Offset, entry.GetByte(), i.Offset, size);
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