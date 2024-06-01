using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace SQLInterpreter.Properties.FileCore
{
    /// <summary>
    /// для работы с таблицей
    /// </summary>
    public class Table
    {
        private string _name;

        public Table(string name)
        {
            _name = name;
        }
        /// <summary>
        /// метод добавления нового поля в таблицу
        /// </summary>
        /// <param name="field">поле таблицы</param>
        public void AddColumn(DbfField field)
        {
            string timedFileName = "9821383831.dbf";
            FileInfo fileInfo = new FileInfo(timedFileName);
            EntryVirtualArray array = new EntryVirtualArray(_name);
            EntryVirtualArray timedArray = new EntryVirtualArray(timedFileName, new DbfHeader(array.Header.GetByte()));
            timedArray.Header.Count = 0;
            timedArray.Header.AddField(field);
            for (int i = 0; i < array.Header.Count; i++)
            {
                Entry entry = array[i];
                timedArray.AppendEntry(Entry.ConvertEntry(entry, timedArray.Header));
            }
            timedArray.Close();
            array.Close();
            File.Delete(_name);
            fileInfo.MoveTo(_name);
        }
        /// <summary>
        /// метод удаления поля из таблицы
        /// </summary>
        /// <param name="columnName">имя удаляемого поля</param>
        public void RemoveColumn(string columnName)
        {
            string timedFileName = "9821383831.dbf";
            FileInfo fileInfo = new FileInfo(timedFileName);
            EntryVirtualArray array = new EntryVirtualArray(_name);
            EntryVirtualArray timedArray = new EntryVirtualArray(timedFileName, new DbfHeader(array.Header.GetByte()));
            timedArray.Header.Count = 0;
            timedArray.Header.RemoveField(columnName);
            for (int i = 0; i < array.Header.Count; i++)
            {
                Entry entry = array[i];
                timedArray.AppendEntry(Entry.ConvertEntry(entry, timedArray.Header));
            }
            timedArray.Close();
            array.Close();
            File.Delete(_name);
            fileInfo.MoveTo(_name);
        }
        /// <summary>
        /// метод переименование поля таблицы
        /// </summary>
        /// <param name="oldColumnName">старое имя</param>
        /// <param name="newColumnName">новое имя</param>
        public void RenameColumn(string oldColumnName, string newColumnName)
        {
            EntryVirtualArray array = new EntryVirtualArray(_name);
            array.Header.RenameField(oldColumnName,newColumnName);
            array.Close();
        }
        /// <summary>
        /// обновляет поле таблицы
        /// </summary>
        /// <param name="field">поле</param>
        public void UpdateColumn(DbfField field)//ваша реализация Entry.UpdateEntry учитывающая приведение типов
        {
            string timedFileName = "9821383831.dbf";
            FileInfo fileInfo = new FileInfo(timedFileName);
            EntryVirtualArray array = new EntryVirtualArray(_name);
            EntryVirtualArray timedArray = new EntryVirtualArray(timedFileName, new DbfHeader(array.Header.GetByte()));
            timedArray.Header.Count = 0;
            timedArray.Header.UpdateField(field);
            for (int i = 0; i < array.Header.Count; i++)
            {
                Entry entry = array[i];
                timedArray.AppendEntry(Entry.UpdateEntry(entry, timedArray.Header));
            }
            timedArray.Close();
            array.Close();
            File.Delete(_name);
            fileInfo.MoveTo(_name);
        }
        /// <summary>
        /// метод для физического удаления помеченных записей
        /// </summary>
        public void Truncate()
        {
            EntryVirtualArray array = new EntryVirtualArray(_name);
            int endIndex = array.Header.Count - 1;
            for (int i = 0; i < array.Header.Count; i++)
            {
                if (array.GetEntryStatus(i)[0] == Constants.Delete)
                {
                    for (; endIndex > i; endIndex--)
                    {
                        if (array.GetEntryStatus(endIndex)[0] == Constants.Delete)
                        {
                            array.WriteEnd(endIndex);
                            array.Header.Count--;
                        }
                        else if(array.GetEntryStatus(endIndex)[0] == Constants.NoDelete)
                        {
                            array[i]=array[endIndex];
                            array.WriteEnd(endIndex);
                            endIndex--;
                            break;
                        }
                    }
                    if (endIndex <= i)
                    {
                        array.WriteEnd(i);
                        array.Header.Count--;
                        break;
                    }
                    array.Header.Count--;
                }
            }
            array.Close();
        }


        /// <summary>
        /// Возвращает записи подходящие под логическое выражение
        /// </summary>
        public List<Entry> Where(LogicEntries logicEntries)
        {
            List<Entry> entries = new List<Entry>() { }; //Подходящие под условие записи
            List<string> fieldsName = new List<string>() { };//Список имен полей
            List<char> fieldsType = new List<char>() { };  //Список типов этих полей соотственно
            List<string> entry; //Значения проверяемой в данный момент записи

            EntryVirtualArray array = new EntryVirtualArray(_name); //Записи
            List<DbfField> fields = array.Header.Fields; //Список полей

            for (int i = 0; i < fields.Count; i++)
            {
                fieldsName.Add(fields[i].Name);
                fieldsType.Add(fields[i].Type);
            }

            logicEntries.CreateCalcSample(fieldsName, fieldsType); //Создаем шаблон для проверки записи

            for (int i = 0; i < array.Header.Count; i++)
            {
                //Перебираем каждую запись
                entry = EntryToStringList(array[i]);
                if (logicEntries.GetResult(entry))
                {
                    entries.Add(array[i]);
                }
            }
            return entries;
        
        }

        /// <summary>
        /// Возвращает записи в строковом виде. Первый элеменет возвращаемого списка - названия полей
        /// </summary>
        public List<List<string>> Select(List<Entry> entries, List<string> fieldsName)
        {
            EntryVirtualArray array = new EntryVirtualArray(_name); //Записи
            List<DbfField> fields = array.Header.Fields; //Список полей
            List<int> indexs = new List<int>() { };
            int index;

            List<string> fieldsNameString = new List<string>() { };//Список имен полей в строком виде
            List<List<string>> stringEntries = new List<List<string>> { };//Возвращаемый список

            //Запишем названия полей в строковый список
            for (int i = 0; i < fields.Count; i++)
            {
                fieldsNameString.Add(fields[i].Name);
            }

            if (fieldsName.Count == 1 && fieldsName[0] == "*") //Если берем все поля
            {

                //Добавляем заголовок
                stringEntries.Add(fieldsNameString);


                //Добавляем сами записи
                for (int i = 0; i < entries.Count; i++)
                {
                    stringEntries.Add(EntryToStringList(entries[i]));
                }
            }

            else //Если берем какие-то конкретные
            {
                //Сначала запомним индексы полей, которые нам надо вернуть, а так же проверим что они существуют            
                for (int i = 0; i < fieldsName.Count; i++)
                {
                    index = fieldsNameString.IndexOf(fieldsName[i]);
                    if (index == -1) throw new Exception("Нет такого поля");
                    indexs.Add(index);
                }

                //Добавляем заголовок
                stringEntries.Add(fieldsName);


                //Добавляем сами записи
                for (int i = 0; i < entries.Count; i++)
                {
                    stringEntries.Add(EntryToStringList(entries[i], indexs));
                }
            }          
            return stringEntries;
        }

        /// <summary>
        /// Убирает пометку удаления у записей (востанавливает)
        /// </summary>
        public int Restore(List<Entry> entries) {
            int k = 0;

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].IsDeleted)
                {
                    k++;
                    entries[i].IsDeleted = false;
                }

            }
            return k;
        }

        /// <summary>
        /// Ставит пометку удаления у записей (удаляет)
        /// </summary>
        public int Delete(List<Entry> entries)
        {
            int k = 0;

            for (int i = 0; i < entries.Count; i++)
            {
                if (!entries[i].IsDeleted)
                {
                    k++;
                    entries[i].IsDeleted = true;
                }

            }
            return k;
        }


        /// <summary>
        /// Перевод запись в строковый спискок
        /// </summary>
        public List<string> EntryToStringList(Entry entry)
        {
            List<string> entryList = new List<string>() { };
            List<DbfField> fields = entry.Header.Fields; //Список полей
            List<byte> data = new List<byte> { };
           

            for (int i = 0; i < fields.Count; i++)
            {
                for (int j = fields[i].Offset; j < fields[i].Offset + fields[i].Size; j++) {
                    data.Add(entry.GetByte()[j]);
                }  
                entryList.Add(Encoding.ASCII.GetString(data.ToArray()));
                data.Clear();
            }
            return entryList;
        }

        /// <summary>
        /// Перевод запись в строковый спискок и возращает только элементы указанные в списке индексов
        /// </summary>
        public List<string> EntryToStringList(Entry entry, List<int> indexs)
        {
            List<string> entryList = new List<string>() { };
            List<DbfField> fields = entry.Header.Fields; //Список полей
            List<byte> data = new List<byte> { };


            for (int i = 0; i < indexs.Count; i++)
            {
                for (int j = fields[indexs[i]].Offset; j < fields[indexs[i]].Offset + fields[indexs[i]].Size; j++)
                {
                    data.Add(entry.GetByte()[j]);
                }
                entryList.Add(Encoding.ASCII.GetString(data.ToArray()));
                data.Clear();
            }
            return entryList;
        }



    }
}