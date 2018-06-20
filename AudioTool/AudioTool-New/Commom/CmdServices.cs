using AudioToolNew.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace AudioToolNew.Commom
{
    public class CmdServices
    {
        static string FFMPEG = HttpContext.Current.Server.MapPath("/exe/") + "ffmpeg.exe";
        static string SILK_V3_DECODER = HttpContext.Current.Server.MapPath("/exe/") + "silk_v3_decoder.exe";

        private static void cmdVoid(string exePath, string cmdStr)
        {
            Process p = new Process();
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(exePath, cmdStr);
            p.StartInfo = myProcessStartInfo;
            try
            {
                if (p.Start())//开始进程
                {
                    while (!p.HasExited)
                    {
                        p.WaitForExit();
                    }
                    p.Close();
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("cmdStr失败:"+e.Message);
                throw new Exception(e.Message);
            }
            finally
            {
                if (p != null)
                { p.Close(); }
            }
        }

        public static string silk2wav(string silkPath)
        {
            string pcmPath = CommonServices.createFileFullPath("pcm");
            string wavPath = CommonServices.createFileFullPath("wav");
            try
            {
                cmdVoid(CmdServices.SILK_V3_DECODER, silkPath + " " + pcmPath);
                //cmdVoid(CmdServices.FFMPEG, "-f s16le -y -ar 24000 -ac 1 -i " + pcmPath + " "+ wavPath);//ffmpeg pcm转wav,微信小程序生成的silk文件转pcm文件的采样率为24000，24000的采样率是固定的，这样生成的wav语速是正常的
                cmdVoid(CmdServices.FFMPEG, "-f s16le -y -ar 24000 -ac 1 -i " + pcmPath + "  -ar 16000 " + wavPath);
                //string wavPath1 = CommonServices.createFileFullPath("wav");                
                //cmdVoid(CmdServices.FFMPEG, "-i "+wavPath + " -ar 16000 " + wavPath1);//采样率为24000的wav转16000的wav,便于百度语音识别
                return wavPath;
            }
            catch (Exception e)
            {
                throw new Exception("CmdServices.silk2wav error:" + e.Message);
            }

        }

        public static string webm2wav(string webmPath)
        {
            string wavPath = CommonServices.createFileFullPath("wav");
            try
            {
                cmdVoid(CmdServices.FFMPEG, "-i " + webmPath + " -ar 16000 " + wavPath);// -ar 16000 设置采样率，因为百度语音识别采样率为8000/16000                
            }
            catch (Exception e)
            {
                throw new Exception("CmdServices.webm2wav error:" + e.Message);
            }
            return wavPath;
        }
        public static string mp32wav(string mp3Path)
        {
            string wavPath = CommonServices.createFileFullPath("wav");
            try
            {
                cmdVoid(CmdServices.FFMPEG, "-i " + mp3Path + " -ar 16000 " + wavPath);// -ar 16000 设置采样率，因为百度语音识别采样率为8000/16000                
            }
            catch (Exception e)
            {
                throw new Exception("CmdServices.mp32wav error:" + e.Message);
            }
            return wavPath;
        }
    }
}