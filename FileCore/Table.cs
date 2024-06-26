﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using SQLInterpreter.FileCore;
using SQLInterpreter.Types;

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
        ///   /// <param name="fieldData">данные полей</param>
        public void AddEntry(string[] fieldsHeaders, string[] fieldData)
        {
            EntryVirtualArray entryVirtualArray = null;
            try
            {
                //открываем таблицу
                entryVirtualArray = new EntryVirtualArray(_name);
                if (fieldsHeaders.Length != fieldData.Length) throw new ArgumentException("Синтаксическая ошибка");
                DbfHeader header = entryVirtualArray.Header;
                Entry newEntry = new Entry(header);
                for (int i = 0; i < fieldsHeaders.Length; i++)
                {
                    var currfield = header.Fields.Find(x => x.Name == fieldsHeaders[i]);//находим тип текущего поля
                    if (currfield == null) throw new ArgumentException("Поле с именем " + fieldsHeaders[i] + " не найдено.");
                    if (currfield.Type == 'D')
                    {
                        Date date = new Date(fieldData[i]);
                        newEntry.Update(fieldsHeaders[i], date.ToByteArray());
                    }
                    else
                    if (currfield.Type == 'L')// добавление логического типа данных 
                    {
                        if (fieldData[i].Equals("t") || fieldData[i].Equals("T"))
                            newEntry.Update(fieldsHeaders[i], Encoding.ASCII.GetBytes("t"));
                        else
                            if (fieldData[i].Equals("f") || fieldData[i].Equals("F"))
                            newEntry.Update(fieldsHeaders[i], Encoding.ASCII.GetBytes("f"));
                        else
                            if (fieldData[i].Equals("n") || fieldData[i].Equals("N"))
                            newEntry.Update(fieldsHeaders[i], Encoding.ASCII.GetBytes("n"));
                        else throw new ArgumentException("Несоответствие значения " + fieldData[i] + " типу поля " + fieldsHeaders[i]);

                    }
                    else
                    if (currfield.Type == 'N' || currfield.Type == 'C') // добавление числовых и сивольных данных 
                    {

                        if (currfield.Type == 'N')
                        {
                            if (!NumberStringCheck.IsValidNumberString(fieldData[i], currfield.Size, currfield.Accuracy))
                            {
                                throw new ArgumentException("Неверный формат данных числового поля");
                            }
                            fieldData[i] = NumberStringCheck.FormatString(fieldData[i], currfield.Size, currfield.Accuracy);
                        }
                        
                        newEntry.Update(fieldsHeaders[i], Encoding.ASCII.GetBytes(fieldData[i]));
                    }
                    else
                    if (currfield.Type == 'M') // мемо файлы
                    {
                        fieldData[i] = fieldData[i].TrimStart('@');
                        uint index = WriteToMemo(fieldData[i], _name);
                        newEntry.Update(fieldsHeaders[i], Encoding.ASCII.GetBytes(index.ToString()));
                    }

                }

                entryVirtualArray.AppendEntry(newEntry);
                entryVirtualArray.Close();
            }
            catch (Exception ex)
            {
                entryVirtualArray.Close();
                throw ex;
            }
        }
        /// <summary>
        /// метод добавления данных в memo файл
        /// </summary>
        /// <param name="filePath">путь</param>
        ///   /// <param name="tableName">имя файла</param>
        private uint WriteToMemo(string filePath,string tableName)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            string textData;
            
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

           
            string dbtfilePath = tableName.Substring(0, tableName.Length - 4) + ".dbt";
            //if (File.Exists(dbtfilePath)){
            DbtFile dbtfile = new DbtFile(dbtfilePath);            
           //}
           // else {
                // dbtfile = new DbtFile(dbtfilePath);
           // }
            dbtfile.AddData(Encoding.ASCII.GetBytes(textData));
            uint blockIndex = dbtfile.Header.NextFreeBlock-1;
            dbtfile.Close();
            return blockIndex;
        }


        /// <summary>
        /// метод добавления нового поля в таблицу
        /// </summary>
        /// <param name="field">поле таблицы</param>
        public void AddColumn(DbfField field)
        {
            FileInfo fileInfo=null;
            EntryVirtualArray array=null;
            EntryVirtualArray timedArray = null;
            string timedFileName = "9821383831.dbf";
            try
            {
               
                fileInfo = new FileInfo(timedFileName);
                array = new EntryVirtualArray(_name);
                timedArray = new EntryVirtualArray(timedFileName, new DbfHeader(array.Header.GetByte()));
                timedArray.Header.Count = 0;
                uint nextFreeBlock=0;
                //timedArray.Header.AddField(field);
                timedArray.Header.AddField(field);
                if (field.Type == 'M')
                {
                    if (array.Header.HasMemo) throw new ArgumentException("Нельзя добавить второе мемо поле.");
                    DbtFile emptyDbtFile = new DbtFile(_name.Substring(0,_name.Length-4) + ".dbt");
                    //nextFreeBlock = emptyDbtFile.Header.NextFreeBlock;
                    emptyDbtFile.Close();
                }
                
                for (int i = 0; i < array.Header.Count; i++)
                {
                    Entry entry = array[i];
                    Entry newEntry = Entry.ConvertEntry(entry, timedArray.Header);
                    if (field.Type == 'M')
                    {
                        newEntry.Update(field.Name, Encoding.ASCII.GetBytes("0"));
                    }
                    timedArray.AppendEntry(newEntry);  
                }
            }
            catch (Exception ex)
            {
                timedArray.Close();
                array.Close();
                File.Delete(timedFileName);
                //fileInfo.MoveTo(_name);
                throw ex;
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
            try
            {
                timedArray.Header.Count = 0;
                timedArray.Header.RemoveField(columnName);
                for (int i = 0; i < array.Header.Count; i++)
                {
                    Entry entry = array[i];
                    timedArray.AppendEntry(Entry.ConvertEntry(entry, timedArray.Header));
                } 
                timedArray.Close();
                array.Close();
                if (!timedArray.Header.HasMemo && array.Header.HasMemo)
                {
                    File.Delete(_name.Substring(0, _name.Length - 4) + ".dbt");
                }
                File.Delete(_name);
                fileInfo.MoveTo(_name);
            }
            catch (Exception ex)
            {
                timedArray.Close();
                array.Close();
                File.Delete(timedFileName);
                //fileInfo.MoveTo(_name);
                throw ex;
            }
        }
        /// <summary>
        /// метод переименование поля таблицы
        /// </summary>
        /// <param name="oldColumnName">старое имя</param>
        /// <param name="newColumnName">новое имя</param>
        public void RenameColumn(string oldColumnName, string newColumnName)
        {
            EntryVirtualArray array = null;
            try
            {
                array = new EntryVirtualArray(_name);
                array.Header.RenameField(oldColumnName, newColumnName);
            }
            catch (Exception ex)
            {
                array.Close();
                throw ex;
            }
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
            try
            {
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
            catch(Exception ex)
            {
                timedArray.Close();
                array.Close();
                File.Delete(timedFileName);
                //fileInfo.MoveTo(_name);
                throw ex; 
            }
           
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
                        //array.WriteEnd(i+1);
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
            try {
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

            }
            catch (Exception ex)
            {
                array.Close();
                throw ex;
            }
            array.Close();
            return entries;
        }


    }
}