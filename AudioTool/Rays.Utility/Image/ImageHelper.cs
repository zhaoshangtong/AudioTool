using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rays.Utility
{
    /// <summary>
    /// 海报图片帮助类
    /// </summary>
    public class ImageHelper
    {
        /// <summary>
        /// 从网络地址获取图片
        /// </summary>
        /// <param name="url"></param>
        public static Bitmap GetBitmapFromUrl(string url)
        {
            Bitmap img = null;
            try
            {
                System.Net.WebClient wc = new System.Net.WebClient();
                byte[] buf = wc.DownloadData(url);
                MemoryStream ms = new MemoryStream(buf);
                img = (Bitmap)System.Drawing.Bitmap.FromStream(ms);   //img就是你要的Bitmap.
                ms.Close();
                wc.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }
            return img;
        }
        #region C#图片处理 合并图片
        /// <summary>
        /// 调用此函数后使此两种图片合并，类似相册，有个
        /// 背景图，中间贴自己的目标图片
        /// </summary>
        /// <param name="sourceImg">粘贴的源图片</param>
        /// <param name="destImg">粘贴的目标图片</param>
        /// 使用说明： string pic1Path = Server.MapPath(@"\testImg\wf.png");
        /// 使用说明： string pic2Path = Server.MapPath(@"\testImg\yj.png");
        /// 使用说明： System.Drawing.Image img = CombinImage(pic1Path, pic2Path);
        /// 使用说明：img.Save(Server.MapPath(@"\testImg\Newwf.png"));
        public static System.Drawing.Image CombinImage(string sourceImg, string destImg)
        {
            System.Drawing.Image imgBack = System.Drawing.Image.FromFile(sourceImg);     //相框图片 
            System.Drawing.Image img = System.Drawing.Image.FromFile(destImg);        //照片图片



            //从指定的System.Drawing.Image创建新的System.Drawing.Graphics       
            Graphics g = Graphics.FromImage(imgBack);

            g.DrawImage(imgBack, 0, 0, 148, 124);      // g.DrawImage(imgBack, 0, 0, 相框宽, 相框高);
            g.FillRectangle(System.Drawing.Brushes.Black, 16, 16, (int)112 + 2, ((int)73 + 2));//相片四周刷一层黑色边框



            //g.DrawImage(img, 照片与相框的左边距, 照片与相框的上边距, 照片宽, 照片高);
            g.DrawImage(img, 17, 17, 112, 73);
            GC.Collect();
            return imgBack;
        }
        #endregion
    }
}
