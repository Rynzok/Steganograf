﻿using System;
using System.IO;
using System.Collections.Generic;
using NAudio.Wave;
using System.Runtime.InteropServices;
using System.Linq;

namespace Steganograf
{
    public class Wave
    {
        private static Dictionary<int, Type> types = new Dictionary<int, Type>
    {
        { 1, typeof(sbyte) },
        { 2, typeof(short) },
        { 4, typeof(int) }
    };

        private WaveFileReader wavein;
        public int channelsNum;
        private int bytesPerSample;
        public int frameRate;
        public int framesNum;
        private Array content;
        public List<List<byte>> channels;
        public List<byte> stego;
        private int switching;

        private int decreasingFrom;
        private int increasingTo;

        public Wave(string inputWav)
        {
            wavein = new WaveFileReader(inputWav);
            channelsNum = wavein.WaveFormat.Channels; // mono / stereo
            bytesPerSample = wavein.WaveFormat.AverageBytesPerSecond; // 1 / 2 / 4
            frameRate = wavein.WaveFormat.SampleRate; // 8000 / 44100 / 48000 / 96000
            framesNum = (int)wavein.Length / wavein.WaveFormat.BlockAlign;
            content = ReadFrames(wavein, framesNum);
            wavein.Close();

            channels = new List<List<byte>>();
            for (int n = 0; n < channelsNum; n++)
            {
                channels.Add(ExtractChannel(content, n, channelsNum));
            }

            stego = new List<byte>();
            switching = 3 * frameRate;

            decreasingFrom = 0;
            increasingTo = 0;
        }

        private void SetDecreasingFrom(Key key)
        {
            decreasingFrom = Math.Max(0, key.begin - switching) * channelsNum;
        }

        private void SetIncreasingTo(Key key)
        {
            increasingTo = Math.Min(framesNum, key.end + switching) * channelsNum;
        }

        private byte[] SetAmplitude(IEnumerable<byte> instAmp)
        {
            List<byte> content = new List<byte>();
            foreach (int a in instAmp)
            {
                byte[] ampInBytes = BitConverter.GetBytes((short)a);
                content.AddRange(ampInBytes);
            }
            return content.ToArray();
        }

        public List<byte> UniteChannels(List<List<byte>> channels)
        {
            List<byte> content = new List<byte>();
            foreach (var channel in channels)
            {
                for (int i = 0; i < channel.Count; i++)
                {
                    content.Add((byte)channel[i]);
                }
            }
            return content;
        }

        private double DecSignal(int i, int begin)
        {
            if (channelsNum == 2 && i % 2 == 1)
                return 1.0;
            return 1.0 - 0.2 * i / (begin - decreasingFrom);
        }

        private double IncSignal(int i, int end)
        {
            if (channelsNum == 2 && i % 2 == 1)
                return 1.0;
            return 0.8 + 0.2 * i / (increasingTo - end);
        }

        public void CreateStegoaudio(Key key)
        {
            string outputWav = "C:\\Users\\vkise\\OneDrive\\Рабочий стол\\Диплом\\out.wav";
            using (WaveFileWriter waveOut = new WaveFileWriter(outputWav, wavein.WaveFormat))
            {
                SetDecreasingFrom(key);
                SetIncreasingTo(key);

                waveOut.Write((byte[])content, 0, decreasingFrom);
                /*  waveOut.WriteBytes(GetSubArray(content, 0, decreasingFrom));  */      // begin
                List<byte> list = content.Cast<byte>().ToList();
                var dec = GetDecreasingAmplitude(list, decreasingFrom, key.begin * channelsNum);
                /*  waveOut.WriteByte(SetAmplitude(dec)); */       // decreasing
                waveOut.Write(SetAmplitude(dec), 0, SetAmplitude(dec).Length);
                waveOut.Write(SetAmplitude(stego), 0, SetAmplitude(stego).Length);

                var inc = GetIncreasingAmplitude(list, key.end * channelsNum, increasingTo, channelsNum);
                waveOut.Write(SetAmplitude(inc), 0, SetAmplitude(inc).Length);        // increasing
                waveOut.Write((byte[])content, increasingTo, content.Length - increasingTo);      // end
            }
        }

        private static Array ReadFrames(WaveFileReader reader, int frames)
        {
            byte[] buffer = new byte[frames * reader.WaveFormat.BlockAlign];
            int read = reader.Read(buffer, 0, buffer.Length);
            //return ConvertByteArrayToArray(buffer, types[reader.WaveFormat.BitsPerSample / 8]);
            return buffer;
        }

        private static List<byte> ExtractChannel(Array content, int channel, int channels)
        {
            //Array channelContent = Array.CreateInstance(types[channels], content.Length / channels);
            List<byte> channelContent = new List<byte>();
            for (int i = channel; i < content.Length; i += channels)
            {
                //int index = i / channels;
                //if (i == 0)
                //{
                //    index = 0;
                //}
                //channelContent.SetValue(content.GetValue(i), index);
                channelContent.Add((byte)content.GetValue(i));
            }
            return channelContent;
        }

        //private static Array GetSubArray(Array array, int start, int length)
        //{
        //    Array subArray = Array.CreateInstance(array.GetType().GetElementType(), length);
        //    Array.Copy(array, start, subArray, 0, length);
        //    return subArray;
        //}

        private static List<byte> GetDecreasingAmplitude(List<byte> content, int start, int end)
        {
            List<byte> amplitude = new List<byte>();
            for (int i = start; i < end; i++)
            {
                int value = content[i];
                amplitude.Add((byte)(value * (1.0 - 0.2 * (i - start) / (end - start))));
            }
            return amplitude;
        }

        private static List<byte> GetIncreasingAmplitude(List<byte> content, int start, int end, int channels)
        {
            List<byte> amplitude = new List<byte>();
            for (int i = start; i < end; i++)
            {
                int value = content[i];
                amplitude.Add((byte)(value * (0.8 + 0.2 * (i - start) / (end - start))));
            }
            return amplitude;
        }

        private static Array ConvertByteArrayToArray(byte[] bytes, Type type)
        {
            int length = bytes.Length / Marshal.SizeOf(type);
            Array array = Array.CreateInstance(type, length);
            Buffer.BlockCopy(bytes, 0, array, 0, bytes.Length);
            return array;
        }
    }
}
