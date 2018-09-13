using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace AudioToolNew.Commom
{
    /// <summary>
    /// ffmpeg处理音频，主要用于转化格式,音频截取，获取音频时长
    /// </summary>
    public class FfmpegHelper
    {
        //音频文件截取指定时间部分
        //ffmpeg64.exe -i 124.mp3 -vn -acodec copy -ss 00:00:00 -t 00:01:32 output.mp3
        //解释：-i代表输入参数
        //-acodec copy output.mp3 重新编码并复制到新文件中
        //-ss 开始位置
        //-t 持续时间
        //-to 位置（输入/输出）停止写入输出或在位置读取输入

        //音频文件格式转换
        //ffmpeg64.exe -i null.ape -ar 44100 -ac 2 -ab 16k -sample_fmt s16 -vol 50 -f mp3 null.mp3
        //解释：-i代表输入参数
        //-acodec aac（音频编码用AAC） 
        //-ar 设置音频采样频率
        //-ac 设置音频通道数
        //-ab 设定声音比特率
        //-vol<百分比> 设定音量
        //sample_fmt 音频采样格式如下
        //name   depth
        //u8        8
        //s16      16
        //s32      32
        //flt      32
        //dbl      64
        //u8p       8
        //s16p     16
        //s32p     32
        //fltp     32
        //dblp     64

        //1、ffmpeg命令：wav转pcm:

        //ffmpeg -i
        //input.wav -f s16be -ar 8000 -acodec pcm_s16be
        //output.raw

        //2、ffmpeg命令：pcm转wav:

        //ffmpeg -f s16be -ar 8000 -ac 2 -acodec pcm_s16be -i
        //input.raw output.wav

        //wav转pcm
        //ffmpeg.exe -i test.wav -f s16be -ar 8000 -acodec cpm_s16be 1.pcm

        //wav转amr
        //ffmpeg -i test.wav -acodec libamr_nb -ab 12.2k -ar 8000 -ac 1 1.amr

        //wav转mp3
        //ffmpeg -i test.wav 1.mp3


        /// <summary>
        /// 音频截取
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="start"></param>
        /// <param name="continues"></param>
        /// <returns></returns>
        public static bool CutAudioFile(string input, string output,TimeSpan start,TimeSpan continues)
        {
            string ffmpegPath = System.AppDomain.CurrentDomain.BaseDirectory + "MediaConvert\\";
            string error = "";
            string szExeFilePath = ffmpegPath + "ffmpeg.exe ";
            Process p = new Process();
            p.StartInfo.FileName = szExeFilePath;
            p.StartInfo.Arguments = " -i " + input + " -vn -acodec copy -ss " + start + " -t " + continues +" "+ output;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;     //重定向输入（一定是true） 
            p.StartInfo.RedirectStandardOutput = true;    //重定向输出    
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;
            try
            {
                if (p.Start())//开始进程
                {
                    error = p.StandardError.ReadToEnd();//读取进程的输出,StandardOutput不行，因为ffmpeg输出都是StandardError
                    p.WaitForExit();
                    p.Close();
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                if (p != null)
                { p.Close(); }
            }
            if (System.IO.File.Exists(output))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 音频截取
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="start"></param>
        /// <param name="continues"></param>
        /// <returns></returns>
        public static bool CutAudioFile2(string input, string output, TimeSpan start, TimeSpan end)
        {
            string ffmpegPath = System.AppDomain.CurrentDomain.BaseDirectory + "MediaConvert\\";
            string error = "";
            string szExeFilePath = ffmpegPath + "ffmpeg.exe ";
            Process p = new Process();
            p.StartInfo.FileName = szExeFilePath;
            p.StartInfo.Arguments = " -i " + input + " -vn -acodec copy -ss " + start + " -to " + end + " " + output;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;     //重定向输入（一定是true） 
            p.StartInfo.RedirectStandardOutput = true;    //重定向输出    
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;
            try
            {
                if (p.Start())//开始进程
                {
                    error = p.StandardError.ReadToEnd();//读取进程的输出,StandardOutput不行，因为ffmpeg输出都是StandardError
                    p.WaitForExit();
                    p.Close();
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                if (p != null)
                { p.Close(); }
            }
            if (System.IO.File.Exists(output))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 音频格式转换
        /// </summary>
        /// <param name="input">要转的文件</param>
        /// <param name="output">转出的文件</param>
        /// <param name="channel">声道，默认1</param>
        /// <param name="rate">采样率，默认16kHz</param>
        /// <param name="bit">音频采样格式，16bit</param>
        /// <returns></returns>
        public static bool ChangeFileType(string input,string output,int channel=1,int rate=16000,int bit= 16)
        {
            string bits = Enum.GetName(typeof(SampleFmt), bit);
            
            string error = "";
            string ffmpegPath = System.AppDomain.CurrentDomain.BaseDirectory + "MediaConvert\\";
            string szExeFilePath = ffmpegPath + "ffmpeg.exe ";
            Process p = new Process();
            p.StartInfo.FileName = szExeFilePath;
            p.StartInfo.Arguments = " -i " + input + " -ar " + rate + " -ac " + channel + " -sample_fmt " + bits + " -f wav " + output;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;     //重定向输入（一定是true） 
            p.StartInfo.RedirectStandardOutput = true;    //重定向输出    
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;
            try
            {
                if (p.Start())//开始进程
                {
                    error = p.StandardError.ReadToEnd();//读取进程的输出,StandardOutput不行，因为ffmpeg输出都是StandardError
                    p.WaitForExit();
                    p.Close();
                }
            }
            catch (Exception e)
            {
                
            }
            finally
            {
                if (p != null)
                { p.Close(); }
            }
            if (System.IO.File.Exists(output))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获得音频时长
        /// </summary>
        /// <param name="sFile">音频文件</param>
        /// <returns></returns>
        public static string getMediaDuration(string sFile)
        {
            string ffmpegPath = System.AppDomain.CurrentDomain.BaseDirectory + "MediaConvert\\";
            string szExeFilePath = ffmpegPath + "ffmpeg.exe ";
            Process p = new Process();
            p.StartInfo.FileName = szExeFilePath;
            p.StartInfo.Arguments = " -i " + sFile;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;     //重定向输入（一定是true） 
            p.StartInfo.RedirectStandardOutput = true;    //重定向输出    
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;

            string output = "";
            try
            {
                if (p.Start())//开始进程
                {
                    output = p.StandardError.ReadToEnd();//读取进程的输出,StandardOutput不行，因为ffmpeg输出都是StandardError
                    p.WaitForExit();
                    p.Close();

                }
            }
            catch (Exception e)
            {
                output = e.Message;
            }
            finally
            {
                if (p != null)
                { p.Close(); }
            }
            if (output.IndexOf("Duration") > -1)
            {
                output = output.Substring(output.IndexOf("Duration: ") + 10, 11);
            }
            else
            {
                output = "00:00:00.00";
            }
            return output;
        }
        /// <summary>
        /// 音频拼接 ffmpeg64.exe -i "concat:123.mp3|124.mp3" -acodec copy output.mp3
        /// 参考(里面有音频混合):http://blog.sina.com.cn/s/blog_50e610900102vkab.html
        /// </summary>
        /// <param name="inputs">多个输入音频</param>
        /// <param name="output">输出音频</param>
        /// <returns></returns>
        public static bool ComposeAudios(string[] inputs,ref string output)
        {
            output=output.Replace("/", "\\");
            string error = "";
            string ffmpegPath = System.AppDomain.CurrentDomain.BaseDirectory + "MediaConvert\\";
            string szExeFilePath = ffmpegPath + "ffmpeg.exe ";
            Process p = new Process();
            p.StartInfo.FileName = szExeFilePath;
            string command = " concat:";
            foreach(string input in inputs)
            {
                command += input+"|";
            }
            command = command.Substring(0, command.Length - 1)+"";
            command = "-i" + command + " -acodec copy " + output;
            p.StartInfo.Arguments = command;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;     //重定向输入（一定是true） 
            p.StartInfo.RedirectStandardOutput = true;    //重定向输出    
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;
            try
            {
                if (p.Start())//开始进程
                {
                    error = p.StandardError.ReadToEnd();//读取进程的输出,StandardOutput不行，因为ffmpeg输出都是StandardError
                    p.WaitForExit();
                    p.Close();
                }
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                if (p != null)
                { p.Close(); }
            }
            if (System.IO.File.Exists(output))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 音频降噪
        /// 参考:https://www.cnblogs.com/yongfengnice/p/7121946.html
        /// </summary>
        /// <param name="input"></param>
        public static void Denoise(string input)
        {
            string error = "";
            string ffmpegPath = System.AppDomain.CurrentDomain.BaseDirectory + "MediaConvert\\";
            string szExeFilePath = ffmpegPath + "ffmpeg.exe ";
            Process p = new Process();
            p.StartInfo.FileName = szExeFilePath;
            p.StartInfo.Arguments = $"ffplay  -i  {input}  -nr  500";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;     //重定向输入（一定是true） 
            p.StartInfo.RedirectStandardOutput = true;    //重定向输出    
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = false;
            try
            {
                if (p.Start())//开始进程
                {
                    error = p.StandardError.ReadToEnd();//读取进程的输出,StandardOutput不行，因为ffmpeg输出都是StandardError
                    p.WaitForExit();
                    p.Close();
                }
            }
            catch (Exception e)
            {
                
            }
            finally
            {
                if (p != null)
                { p.Close(); }
            }
        }

    }
    enum SampleFmt
    {
        u8=8,
        s16=16,
        s32=32,
        flt= 33,//
        dbl=64,
        u8p=-8,
        s16p=-16,
        s32p=-32,
        fltp=-33,//
        dblp=-64
    }
}