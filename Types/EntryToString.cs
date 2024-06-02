using SQLInterpreter.FileCore;
using SQLInterpreter.Properties.FileCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter.Types
{
    /// <summary>
    /// Класс для работы с записью в строковом формате
    /// </summary>
    public static class EntryToString
    {
        /// <summary>
        /// Перевод записи в строковый спискок и возращает только элементы указанные в списке индексов (не работает с МЕМО)
        /// </summary>
        public static List<string> EntryToStringList(Entry entry, List<int> indexs)
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
                entryList.Add(Encoding.ASCII.GetString(data.ToArray()).Split('\0')[0]);
                data.Clear();
            }
            return entryList;
        }

        /// <summary>
        /// Перевод записи в строковый спискок (не работает с МЕМО)
        /// </summary>
        public static List<string> EntryToStringList(Entry entry)
        {
            List<string> entryList = new List<string>() { };
            List<DbfField> fields = entry.Header.Fields; //Список полей
            List<byte> data = new List<byte> { };

            for (int i = 0; i < fields.Count; i++)
            {
                for (int j = fields[i].Offset; j < fields[i].Offset + fields[i].Size; j++)
                {
                    data.Add(entry.GetByte()[j]);
                }
                entryList.Add(Encoding.ASCII.GetString(data.ToArray()).Split('\0')[0]);
                data.Clear();
            }
            return entryList;
        }

        /// <summary>
        /// Перевод списка записи в строковую матрицу (корректно работает с MEMO). Возращает только элементы указанные в списке индексов
        /// </summary>
        public static List<List<string>> EntryListToStringMatrix(List<Entry>  entres, string tableName, List<int> indexs)
        {
            List<List<string>> entryList = new List<List<string>> { };
            List<DbfField> fields = entres[0].Header.Fields; //Список полей
            List<byte> data = new List<byte> { };
            DbtFile dbtFile;
            uint numberBlock;


            for (int i = 0; i < entres.Count; i++)
            {
                entryList.Add(new List<string> { });
                for (int j = 0; j < indexs.Count; j++)
                {
                    //Если поле является MEMO
                    if (fields[indexs[j]].Type == 'M')
                    {
                        dbtFile = new DbtFile(tableName.Split('.')[0] + ".dbt");
                        //Считываем номер блока
                        for (int k = fields[indexs[j]].Offset; k < fields[indexs[j]].Offset + fields[indexs[j]].Size; k++)
                        {
                            data.Add(entres[i].GetByte()[k]);
                        }
                        numberBlock = Convert.ToUInt32(Encoding.ASCII.GetString(data.ToArray()).Split('\0')[0]);

                        entryList[i].Add(Encoding.ASCII.GetString(dbtFile.GetBlockData(numberBlock)));

                    }

                    //Если поле не МЕМО
                    else {
                        for (int k = fields[indexs[j]].Offset; k < fields[indexs[j]].Offset + fields[indexs[j]].Size; k++)
                        {
                            data.Add(entres[i].GetByte()[k]);
                        }
                        entryList[i].Add(Encoding.ASCII.GetString(data.ToArray()).Split('\0')[0]);      
                    }
                    data.Clear();
                }

            }
            
            return entryList;
        }

        /// <summary>
        /// Перевод списка записи в строковую матрицу (корректно работает с MEMO). 
        /// </summary>
        public static List<List<string>> EntryListToStringMatrix(List<Entry> entres, string tableName)
        {
            List<List<string>> entryList = new List<List<string>> { };
            List<DbfField> fields = entres[0].Header.Fields; //Список полей
            List<byte> data = new List<byte> { };
            DbtFile dbtFile;
            uint numberBlock;


            for (int i = 0; i < entres.Count; i++)
            {
                entryList.Add(new List<string> { });
                for (int j = 0; j < fields.Count; j++)
                {
                    //Если поле является MEMO
                    if (fields[j].Type == 'M')
                    {
                        dbtFile = new DbtFile(tableName.Split('.')[0] + ".dbt"); 
                        //Считываем номер блока
                        for (int k = fields[j].Offset; k < fields[j].Offset + fields[j].Size; k++)
                        {
                            data.Add(entres[i].GetByte()[k]);
                        }
                        numberBlock = Convert.ToUInt32(Encoding.ASCII.GetString(data.ToArray()).Split('\0')[0]);

                        entryList[i].Add(Encoding.ASCII.GetString(dbtFile.GetBlockData(numberBlock)).Split('\0')[0]);

                    }

                    //Если поле не МЕМО
                    else
                    {
                        for (int k = fields[j].Offset; k < fields[j].Offset + fields[j].Size; k++)
                        {
                            data.Add(entres[i].GetByte()[k]);
                        }
                        entryList[i].Add(Encoding.ASCII.GetString(data.ToArray()).Split('\0')[0]);
                    }
                    data.Clear();
                }

            }

            return entryList;
        }

    }
}
