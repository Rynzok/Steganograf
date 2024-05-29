using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;

namespace Steganograf
{
    public class AudioWave
    {
        private WaveFileReader wavein;
        public int frameRate;
        public int framesNum;
        public byte[] content;
        public byte[] stego;
        private int channelsNum;
        public List<byte[]> channels;

        public AudioWave(string inputPath)
        {

            wavein = new WaveFileReader(inputPath);
            channelsNum = wavein.WaveFormat.Channels;
            frameRate = wavein.WaveFormat.SampleRate;
            framesNum = (int)wavein.Length / wavein.WaveFormat.BlockAlign;
            content = ReadFrames(wavein);


            channels = new List<byte[]>();
            for (int n = 0; n < channelsNum; n++)
            {
                channels.Add(ExtractChannel(content, n, channelsNum));
            }
        }

        private static byte[] ReadFrames(WaveFileReader reader)
        {
            byte[] buffer = new byte[reader.Length];
            reader.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        private static byte[] ExtractChannel(Array content, int channel, int channels)
        {
            List<byte> channelContent = new List<byte>();
            for (int i = channel; i < content.Length; i += channels)
            {
                channelContent.Add((byte)content.GetValue(i));

            }
            return channelContent.ToArray();

        }

        public void CreateAudio(byte[] siganal, string name)
        {
            string outputWav = string.Format("C:\\Users\\vkise\\OneDrive\\Рабочий стол\\Диплом\\out_{0}.wav", name);
            using (WaveFileWriter waveOut = new WaveFileWriter(outputWav, wavein.WaveFormat))
            {
                waveOut.Write(siganal, 0, siganal.Length);
            }
        }

        public void CreateStegoaudio()
        {
            byte[] stegoAudio = new byte[content.Length];
            for (int i = 0; i < stegoAudio.Length; i++)
            {
                if (i % 2 == 0)
                {
                    stegoAudio[i] = channels[0][i / 2];
                }
                else
                {
                    stegoAudio[i] = channels[1][i / 2];
                }

            }

            CreateAudio(stegoAudio, "final");
        }
    }
}
