using SQLInterpreter.FileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLInterpreter
{
    internal class FileManagerAdapter
    {
        FileManagerFacade FileManager { get; set; }

        public void Add()
        {
            FileManager.Add();
        }
    }
}
