using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioToolNew.Common
{
    public class WAVReader
    {
        private string Id; //文件标识
        private double Size;  //文件大小
        private string Type; //文件类型
        private string formatId;
        private double formatSize;      //数值为16或18，18则最后又附加信息
        private int formatTag;
        private int num_Channels;       //声道数目
        private int SamplesPerSec;      //采样率
        public int AvgBytesPerSec;     //每秒所需字节数 
        private int BlockAlign;         //数据块对齐单位(每个采样需要的字节数) 
        private int BitsPerSample;      //每个采样需要的bit数
        private string additionalInfo;  //附加信息
        private string dataId;
        private int dataSize;
        public List<double> wavdata = new List<double>();
        public void ReadWAVFile(string filePath)  //读取波形文件并显示
        {
            if (filePath == "") return;
            byte[] id = new byte[4];
            byte[] size = new byte[4];
            byte[] type = new byte[4];
            byte[] formatid = new byte[4];
            byte[] formatsize = new byte[4];
            byte[] formattag = new byte[2];
            byte[] numchannels = new byte[2];
            byte[] samplespersec = new byte[4];
            byte[] avgbytespersec = new byte[4];
            byte[] blockalign = new byte[2];
            byte[] bitspersample = new byte[2];
            byte[] additionalinfo = new byte[2];    //可选
            byte[] factid = new byte[4];
            byte[] factsize = new byte[4];
            byte[] factdata = new byte[4];
            byte[] dataid = new byte[4];
            byte[] datasize = new byte[4];
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs, Encoding.UTF8))
                {
                    #region  RIFF WAVE Chunk
                    br.Read(id, 0, 4);
                    br.Read(size, 0, 4);
                    br.Read(type, 0, 4);
                    Id = getString(id, 4);
                    long longsize = bytArray2Int(size);//十六进制转为十进制
                    Size = longsize * 1.0;
                    Type = getString(type, 4);
                    #endregion
                    #region Format Chunk
                    br.Read(formatid, 0, 4);
                    br.Read(formatsize, 0, 4);
                    br.Read(formattag, 0, 2);
                    br.Read(numchannels, 0, 2);
                    br.Read(samplespersec, 0, 4);
                    br.Read(avgbytespersec, 0, 4);
                    br.Read(blockalign, 0, 2);
                    br.Read(bitspersample, 0, 2);
                    if (getString(formatsize, 2) == "18")
                    {
                        br.Read(additionalinfo, 0, 2);
                        additionalInfo = getString(additionalinfo, 2);  //附加信息
                    }
                    formatId = getString(formatid, 4);
                    formatSize = bytArray2Int(formatsize);
                    byte[] tmptag = composeByteArray(formattag);
                    formatTag = bytArray2Int(tmptag);
                    byte[] tmpchanels = composeByteArray(numchannels);
                    num_Channels = bytArray2Int(tmpchanels);                //声道数目
                    SamplesPerSec = bytArray2Int(samplespersec);            //采样率
                    AvgBytesPerSec = bytArray2Int(avgbytespersec);          //每秒所需字节数   
                    byte[] tmpblockalign = composeByteArray(blockalign);
                    BlockAlign = bytArray2Int(tmpblockalign);              //数据块对齐单位(每个采样需要的字节数)
                    byte[] tmpbitspersample = composeByteArray(bitspersample);
                    BitsPerSample = bytArray2Int(tmpbitspersample);        // 每个采样需要的bit数     
                    #endregion
                    #region Data Chunk
                    byte[] d_flag = new byte[1];
                    while (true)
                    {
                        br.Read(d_flag, 0, 1);
                        if (getString(d_flag, 1) == "d")
                        {
                            break;
                        }
                    }
                    byte[] dt_id = new byte[4];
                    dt_id[0] = d_flag[0];
                    br.Read(dt_id, 1, 3);
                    dataId = getString(dt_id, 4);
                    br.Read(datasize, 0, 4);
                    dataSize = bytArray2Int(datasize);
                    List<string> testl = new List<string>();
                    if (BitsPerSample == 8)
                    {
                        for (int i = 0; i < dataSize; i++)
                        {
                            byte wavdt = br.ReadByte();
                            wavdata.Add(wavdt);
                        }
                    }
                    else if (BitsPerSample == 16)
                    {
                        for (int i = 0; i < dataSize / 2; i++)
                        {
                            short wavdt = br.ReadInt16();
                            wavdata.Add(wavdt);
                        }
                    }
                    #endregion
                }
            } //wavdata


        }
        // 数字节数组转换为int
        private int bytArray2Int(byte[] bytArray)
        {
            return bytArray[0] | (bytArray[1] << 8) | (bytArray[2] << 16) | (bytArray[3] << 24);
        }
        // 将字节数组转换为字符串
        private string getString(byte[] bts, int len)
        {
            char[] tmp = new char[len];
            for (int i = 0; i < len; i++)
            {
                tmp[i] = (char)bts[i];
            }
            return new string(tmp);
        }
        // 组成4个元素的字节数组
        private byte[] composeByteArray(byte[] bt)
        {
            byte[] tmptag = new byte[4] { 0, 0, 0, 0 };
            tmptag[0] = bt[0];
            tmptag[1] = bt[1];
            return tmptag;
        }
        /// <summary>
        /// 获取音频断点时间戳
        /// </summary>
        /// <param name="file_path">文件路径</param>
        /// <param name="splitTime">返回的时间点集合</param>
        public void GetTimeSpan(string file_path,out List<TimeSpan> splitTime, out List<double> voiceMilliSecond,double split)
        {
            splitTime = new List<TimeSpan>();
            voiceMilliSecond = new List<double>();
            try
            {
                ReadWAVFile(file_path);
                //1秒的字节数，16000字节
                double bytesCount = AvgBytesPerSec * split;
                int count = 0;
                int totalCount = 0;
                bool _first = true;//是否第一次进入
                bool _hasVoice = true;//是否有声音
                double startSecond = 0;//计算时间
                foreach (var data in wavdata)
                {
                    ++count;
                    ++totalCount;
                    double _data = data / 1000.0;
                    double max = Math.Abs(_data);//取绝对值,大于1的时候表示有声音
                    double time = Math.Round((double)totalCount / AvgBytesPerSec, 5) * 2;//通过字节数算时间

                    if (_hasVoice && Math.Abs(_data) < 1 && _first)//第一次进入
                    {
                        _first = false;
                        _hasVoice = false;
                    }
                    //无声音时，计算时间
                    else if (!_first && _hasVoice && max < 1 && (time - startSecond) > split)//每次结束对话
                    {
                        _hasVoice = false;
                        splitTime.Add(TimeSpan.FromSeconds(time - split));
                    }
                    else if (!_hasVoice && max > 1)//开始有声音
                    {
                        startSecond = Math.Round((double)totalCount / AvgBytesPerSec, 5) * 2;
                        _hasVoice = true;
                        splitTime.Add(TimeSpan.FromSeconds(startSecond));
                        voiceMilliSecond.Add(startSecond);
                    }
                    else if (_hasVoice && max > 1)//正在播声音
                    {
                        startSecond = Math.Round((double)totalCount / AvgBytesPerSec, 5) * 2;//有声音时继续累加时间
                    }

                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}

