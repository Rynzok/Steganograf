using System;
using System.IO;
using System.Collections.Generic;
using NAudio.Wave;
using System.Runtime.InteropServices;
using System.Linq;

namespace Steganograf
{
    public class Wave
    {
    //    private static Dictionary<int, Type> types = new Dictionary<int, Type>
    //{
    //    { 1, typeof(sbyte) },
    //    { 2, typeof(short) },
    //    { 4, typeof(int) }
    //};

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
                channels.Add(ExtractChannel(content, n, channelsNum).ToList());
            }
            //CreateAll(channels[0], "0");
            //CreateAll(channels[1], "1");
            
            stego = new List<byte>();
            switching = 3 * frameRate;

            decreasingFrom = 0;
            increasingTo = 0;
        }

        private void SetDecreasingFrom(Key key)
        {
            decreasingFrom = Math.Max(0, key.Begin - switching) * channelsNum;
        }

        private void SetIncreasingTo(Key key)
        {
            increasingTo = Math.Min(framesNum, key.End + switching) * channelsNum;
        }

        public List<byte> UniteChannels(List<List<byte>> channels)
        {
            List<byte> content = new List<byte>();
            foreach (var channel in channels)
            {
                for (int i = 0; i < channel.Count; i++)
                {
                    content.Add(channel[i]);
                }
            }
            return content;
        }

        public void CreateStegoaudio(Key key)
        {
            string outputWav = "C:\\Users\\vkise\\OneDrive\\Рабочий стол\\Диплом\\out.wav";
            using (WaveFileWriter waveOut = new WaveFileWriter(outputWav, wavein.WaveFormat))
            {
                SetDecreasingFrom(key);
                SetIncreasingTo(key);

                waveOut.Write((byte[])content, 0, decreasingFrom); // begin

                List<byte> list = content.Cast<byte>().ToList();
                var dec = GetDecreasingAmplitude(list, decreasingFrom, key.Begin * channelsNum);

                waveOut.Write(dec.ToArray(), 0, dec.Count); // decreasing

                waveOut.Write(stego.ToArray(), 0, stego.Count); //Write code signal

                var inc = GetIncreasingAmplitude(list, key.End * channelsNum, increasingTo, channelsNum);
                waveOut.Write(inc.ToArray(), 0, inc.Count);        // increasing

                waveOut.Write((byte[])content, increasingTo, content.Length - increasingTo);      // end
            }
        }

        public void CreateAll(List<byte> siganal, string name)
        {
            string outputWav = string.Format("C:\\Users\\vkise\\OneDrive\\Рабочий стол\\Диплом\\out_{0}.wav", name);
            using (WaveFileWriter waveOut = new WaveFileWriter(outputWav, wavein.WaveFormat))
            {
                waveOut.Write(siganal.ToArray(), 0, siganal.Count);
            }
        }

        private static Array ReadFrames(WaveFileReader reader, int frames)
        {
            byte[] buffer = new byte[frames * reader.WaveFormat.BlockAlign];
            int read = reader.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        private static byte[] ExtractChannel(Array content, int channel, int channelsNum)
        {
            byte[] channelContent = new byte[content.Length / 2];
            for (int i = 0; i < content.Length/2; i ++)
            {
                int index = i;
                //if (i == 0) index = 0;
                if (channel == 1) index = i + content.Length / 2;
                channelContent[i] = (byte)content.GetValue(index);
            }
            return channelContent;
        }


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

    }
}
