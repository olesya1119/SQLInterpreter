using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;
using SQLInterpreter.FileCore;


namespace SQLInterpreter.Properties.FileCore
{
    /// <summary>
    /// для работы с таблицей
    /// </summary>
    public class Table
    {
        private string _name;

        public string Name {  get { return _name; } }

        public Table(string name)
        {
            _name = name;
            EntryVirtualArray entryVirtualArray = new EntryVirtualArray(name);
            entryVirtualArray.Close();
        }

        

        /// <summary>
        /// метод добавления новой строки в таблицу
        /// </summary>
        /// <param name="fieldsHeaders">заголовки полей</param>
        ///   /// <param name="data">данные полей</param>
        public void AddEntry(string[] fieldsHeaders, string[] data)
        {
            if (fieldsHeaders.Length != data.Length) throw new ArgumentException("Синтаксическая ошибка"); 
            EntryVirtualArray entryVirtualArray = new EntryVirtualArray(_name); //открываем таблицу
            DbfHeader header = entryVirtualArray.Header;
            Entry newEntry = new Entry(header);
            for(int i = 0;i< fieldsHeaders.Length; i++)
            {
                var currfield = header.Fields.Find(x => x.Name == fieldsHeaders[i]);//находим тип текущего поля
                if (currfield.Type == 'D')
                {
                    Date date = new Date(data[i]);
                    newEntry.Update(fieldsHeaders[i], date.ToByteArray());
                }
                else
                if (currfield.Type == 'L')// добавление логического типа данных 
                {
                    if (data[i].Equals("t"))
                        newEntry.Update(fieldsHeaders[i], Encoding.ASCII.GetBytes("t"));
                    if (data[i].Equals("f"))
                        newEntry.Update(fieldsHeaders[i], Encoding.ASCII.GetBytes("f"));
                    if (data[i].Equals("n"))
                        newEntry.Update(fieldsHeaders[i], Encoding.ASCII.GetBytes("n"));
                }
                else
                if (currfield.Type == 'N' || currfield.Type == 'C') // добавление числовых и сивольных данных 
                {
                    newEntry.Update(fieldsHeaders[i], Encoding.ASCII.GetBytes(data[i]));
                }
                else
                if(currfield.Type == 'M') // мемо файлы
                {

                }

            }
            
            entryVirtualArray.AppendEntry(newEntry);
            entryVirtualArray.Close();
        }
        private uint WriteToMemo(string filePath,string tableName)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string textData;
            filePath=filePath.TrimStart('@');
            if (fileInfo.Length > 512)
            {
                throw new ArgumentException("Ошибка: Размер текстового файла превышает 512 байт.");
            }
            else
            {
                // Считываем содержимое текстового файла в буфер
                try
                {
                    textData = File.ReadAllText(filePath, Encoding.UTF8);
            
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Ошибка при чтении файла: " + ex.Message);
                }
            }

            DbtFile dbtfile = new DbtFile(tableName+".dbt");
            uint blockIndex = dbtfile.Header.NextFreeBlock;
            dbtfile.AddData(Encoding.ASCII.GetBytes(textData));
            return blockIndex;
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

        public List<Entry> RunForArray(IActivity activity, LogicEntries logicEntries)
        {
            List<Entry> entries = new List<Entry>();
            EntryVirtualArray array = new EntryVirtualArray(_name);
            for (int i = 0; i < array.Header.Count; i++)
            {
                var entry = array[i];
                if (logicEntries.GetResult(entry))
                {
                    activity.Do(entry);
                    entries.Add(entry);
                    array[i] = entry;
                }
            }
            array.Close();
            return entries;
        }




    }
}