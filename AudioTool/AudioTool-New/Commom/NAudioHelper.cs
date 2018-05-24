using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AudioToolNew.Common
{
    public class NAudioHelper
    {
        IWavePlayer waveOutDevice;
        AudioFileReader audioFileReader;
        /// <summary>
        /// 播放MP3
        /// </summary>
        /// <param name="filePath"></param>
        public void Play(string filePath)
        {
            waveOutDevice = new WaveOut();
            audioFileReader = new AudioFileReader(filePath);
            waveOutDevice.Init(audioFileReader);
            waveOutDevice.Play();
            while (waveOutDevice.PlaybackState != PlaybackState.Stopped)
            {
                Thread.Sleep(200);
            }

            waveOutDevice.Stop();
            waveOutDevice.Dispose();
            audioFileReader.Dispose();
        }

        /// <summary>
        /// MP3转wav
        /// </summary>
        /// <param name="filePath"></param>
        public void Mp3toWav(string filePath)
        {
            string outputFileName = filePath.Substring(0, filePath.Length - 3) + "wav";
            using (Mp3FileReader reader = new Mp3FileReader(filePath))
            {
                WaveFileWriter.CreateWaveFile(outputFileName, reader);
            }
        }
        /// <summary>
        /// 将音频文件转换为固定格式的wav（MP3和wav）
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="newFilePath"></param>
        public static AudioFileReader ConvertToWav(string filePath)
        {
            string newFolder = System.AppDomain.CurrentDomain.BaseDirectory + "/NewSoundFiles/" + Path.GetFileNameWithoutExtension(filePath) + "/";
            //重新存储到一个新的文件目录
            if (!System.IO.Directory.Exists(newFolder))
            {
                System.IO.Directory.CreateDirectory(newFolder);
            }
            AudioFileReader outStream = null;
            if (filePath.EndsWith(".wav", StringComparison.CurrentCultureIgnoreCase))
            {
                using (var reader = new WaveFileReader(filePath))
                {
                    var newFormat = new WaveFormat(16000, 16, 1); // 16kHz, 16bit，单声道
                    using (var conversionStream = new WaveFormatConversionStream(newFormat, reader))
                    {
                        string newFilePath = newFolder+Path.GetFileNameWithoutExtension(filePath) + "-new.wav";
                        WaveFileWriter.CreateWaveFile(newFilePath, conversionStream);
                        outStream = new AudioFileReader(newFilePath);
                        return outStream;
                        #region
                        //byte[] buffer = new byte[conversionStream.Length];
                        //conversionStream.Read(buffer, 0, buffer.Length);
                        //Stream stream = new MemoryStream(buffer);
                        //outStream = new AudioFileReader(stream);
                        //return outStream;
                        #endregion
                    }

                }
            }
            else if (filePath.EndsWith(".mp3", StringComparison.CurrentCultureIgnoreCase))
            {
                using (Mp3FileReader reader = new Mp3FileReader(filePath))
                {
                    var newFormat = new WaveFormat(16000, 16, 1); // 16kHz, 16bit，单声道
                    using (var conversionStream = new WaveFormatConversionStream(newFormat, reader))
                    {
                        string newFilePath = newFolder+ Path.GetFileNameWithoutExtension(filePath) + "-new.wav";
                        WaveFileWriter.CreateWaveFile(newFilePath, conversionStream);
                        outStream = new AudioFileReader(newFilePath);
                        return outStream;
                    }
                }
            }
            else
            {
                using (AudioFileReader reader = new AudioFileReader(filePath))
                {
                    var newFormat = new WaveFormat(16000, 16, 1); // 16kHz, 16bit，单声道
                    using (var conversionStream = new WaveFormatConversionStream(newFormat, reader))
                    {
                        string newFilePath = newFolder+ Path.GetFileNameWithoutExtension(filePath) + "-new.wav";
                        WaveFileWriter.CreateWaveFile(newFilePath, conversionStream);
                        outStream = new AudioFileReader(newFilePath);
                        return outStream;
                    }

                }
            }
        }
        /// <summary>
        /// 将音频文件转换为固定格式的wav文件（MP3和wav）
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetWavPath(string filePath)
        {
            string newFolder = System.AppDomain.CurrentDomain.BaseDirectory + "/NewSoundFiles/" + Path.GetFileNameWithoutExtension(filePath) + "/";
            if (!System.IO.Directory.Exists(newFolder))
            {
                System.IO.Directory.CreateDirectory(newFolder);
            }
            string newFilePath = newFolder + Path.GetFileNameWithoutExtension(filePath) + "-new.wav";
            try
            {
                if (filePath.EndsWith(".wav", StringComparison.CurrentCultureIgnoreCase))
                {

                    using (var reader = new WaveFileReader(filePath))
                    {
                        var newFormat = new WaveFormat(16000, 16, 1); // 16kHz, 16bit，单声道
                        using (var conversionStream = new WaveFormatConversionStream(newFormat, reader))
                        {
                            WaveFileWriter.CreateWaveFile(newFilePath, conversionStream);
                        }
                    }
                }
                else if (filePath.EndsWith(".mp3", StringComparison.CurrentCultureIgnoreCase))
                {
                    using (Mp3FileReader reader = new Mp3FileReader(filePath))
                    {
                        var newFormat = new WaveFormat(16000, 16, 1); // 16kHz, 16bit，单声道
                        using (var conversionStream = new WaveFormatConversionStream(newFormat, reader))
                        {
                            WaveFileWriter.CreateWaveFile(newFilePath, conversionStream);
                        }
                    }
                }
                else
                {
                    using (AudioFileReader reader = new AudioFileReader(filePath))
                    {
                        var newFormat = new WaveFormat(16000, 16, 1); // 16kHz, 16bit，单声道
                        using (var conversionStream = new WaveFormatConversionStream(newFormat, reader))
                        {
                            WaveFileWriter.CreateWaveFile(newFilePath, conversionStream);
                        }
                    }

                }
            }
            catch(Exception ex)
            {
                LogHelper.Error("mp3转wav出错："+ex.Message);
            }
            return newFilePath;
        }


        /// <summary>
        /// 将wav转pcm
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetPcmPath(string filePath)
        {
            string newFolder = System.AppDomain.CurrentDomain.BaseDirectory + "/NewSoundFiles/" + Path.GetFileNameWithoutExtension(filePath) + "/";
            if (!System.IO.Directory.Exists(newFolder))
            {
                System.IO.Directory.CreateDirectory(newFolder);
            }
            string newFilePath = newFolder + Path.GetFileNameWithoutExtension(filePath) + "-new.pcm";
            if (filePath.EndsWith(".wav", StringComparison.CurrentCultureIgnoreCase))
            {
                
                using (var reader = new WaveFileReader(filePath))
                {
                    var newFormat = new WaveFormat(16000, 16, 1); // 16kHz, 16bit，单声道
                    using (var conversionStream = WaveFormatConversionStream.CreatePcmStream(reader))
                    {
                        WaveFileWriter.CreateWaveFile(newFilePath, conversionStream);
                    }
                }
            }
            else
            {
                return "";
            }
            return newFilePath;
        }
    }
}

