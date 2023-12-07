using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePhotos
{
    public class FileAccessHelper
    {
        public static string GetLocationFile(string fileName)
        {
            //Regresa dodne se guardan los daots mas el archivo
            return System.IO.Path.Combine(FileSystem.AppDataDirectory, fileName);
        }
    }
}
