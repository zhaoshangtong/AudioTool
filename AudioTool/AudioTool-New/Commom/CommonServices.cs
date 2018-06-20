using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AudioToolNew.Commom
{
    public class CommonServices
    {
        private static string FILE_DIC = HttpContext.Current.Server.MapPath("/upload/");
        public CommonServices()
        {
        }
        public static string createFileFullPath(string fileType)
        {
            string randomStr = sys.getRandomStr();
            string folder = FILE_DIC + sys.getDateStr() + "\\";
            //创建日起戳目录
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            return folder + randomStr + "." + fileType;
        }

        public static string getFileType(string filePath)
        {
            return filePath.Substring(filePath.LastIndexOf(".") + 1).ToLower();
        }
    }
}