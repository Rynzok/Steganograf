using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Steganograf
{
    public class BinaryMessage
    {
        public List<int> bits;
        private StreamReader input;
        public double average;
        public int bitsLength;

        public BinaryMessage(string inputTxt)
        {
            bits = new List<int>();
            input = new StreamReader(inputTxt);
            string inputText = input.ReadToEnd();
            byte[] bytes = Encoding.ASCII.GetBytes(inputText);
            foreach (byte b in bytes)
            {
                string buff = Convert.ToString(b, 2).PadLeft(8, '0');
                foreach (char b2 in buff)
                {
                    if (b2 == '0') bits.Add(0);
                    else bits.Add(1);
                }
            }

            average = bits.Average();
            bitsLength = bits.Count;
            
        }
    }

    public class Key
    {
        public List<int> Delta { get; set; }
        public int Begin { get; set; }
        public int End { get; set; }
        public string Output_txt { get; set; } = "C:\\Users\\vkise\\OneDrive\\Рабочий стол\\Диплом\\key.txt";

        public Key()
        {
            Delta = new List<int>();
            Begin = 0;
            End = 0;
        }

        public void Save()
        {
            using (StreamWriter output = new StreamWriter(Output_txt))
            {
                output.Write(string.Join(" ", Delta[0], Delta[1], Begin, End));
            }
        }
    }

    public class Systema
    {
        public Wave signal;
        private BinaryMessage message;
        public Key key;

        private double echoVolume = 0.3;
        private int hiddenBitsPerSecond = 16;

        private double volumeMax = 0.9;
        private double volumeMin = 0.7;
        private int deltaMax = 30;
        private int deltaMin = 40;

        private double volume0;
        private double volume1;

        private int samplesPerSection;
        private int diff;

        private int samplesPerMessage;
        private List<List<byte>> stegochannels;

        public Systema(Wave signal, BinaryMessage message, Key key)
        {
            this.signal = signal;
            this.message = message;
            this.key = key;

            if (message.average <= 0.5)
            {
                volume0 = volumeMax;
                volume1 = volumeMin;
                key.Delta = new List<int> { deltaMax, deltaMin };
            }
            else
            {
                volume0 = volumeMin;
                volume1 = volumeMax;
                key.Delta = new List<int> { deltaMin, deltaMax };
            }

            samplesPerSection = signal.frameRate / hiddenBitsPerSecond;
            diff = signal.frameRate % hiddenBitsPerSecond;

            samplesPerMessage = CountSamples();
            key.Begin = GetBegin();
            key.End = key.Begin + samplesPerMessage;

            stegochannels = new List<List<byte>>();
        }

        private int CountSamples()
        {
            int divPart = message.bitsLength / hiddenBitsPerSecond * signal.frameRate;
            int modPart = message.bitsLength % hiddenBitsPerSecond * samplesPerSection;
            return divPart + modPart;
        }

        private int GetBegin()
        {
            int rest = signal.framesNum % signal.frameRate;
            int acceptableBegin = signal.framesNum - samplesPerMessage - rest;
            if (acceptableBegin < 0)
            {
                Console.WriteLine("Текст слишком велик для аудиозаписи");
                return -1;
            }
            int maxSecond = acceptableBegin / signal.frameRate;
            int randSecond = new Random().Next((int)Math.Floor(maxSecond * 0.05), maxSecond);
            return randSecond * signal.frameRate;
        }

        private double SmoothingSignal(int i, int position)
        {
            int x = message.bits[i];
            double a = 0.0005;
            double b = 0.9995;
            double k = (double)position / samplesPerSection;
            if ((a < k && k < b) || (a > k && i != 0 && message.bits[i - 1] == x) ||
                (k > b && i + 1 != message.bitsLength && message.bits[i + 1] == x))
            {
                return 1.0;
            }
            if (a >= k)
            {
                return k / a;
            }
            if (k >= b)
            {
                return (1.0 - k) / (1.0 - b);
            }
            return 0.0;
        }

        private double GetEcho(List<byte> channel, int k, int n, int counter)
        {
            double echo0 = volume0 * echoVolume * (k >= key.Delta[0] ? channel[k - key.Delta[0]] : 0) *
                           (1 - SmoothingSignal(counter, k - n));
            double echo1 = volume1 * echoVolume * (k >= key.Delta[1] ? channel[k - key.Delta[1]] : 0) *
                           SmoothingSignal(counter, k - n);
            return echo0 + echo1;
        }

        private List<byte> EmbedStegoMessage(List<byte> channel)
        {
            int secondCounter = key.Begin / signal.frameRate;
            int sectionCounter = 0;
            double volume = 1.0 - echoVolume * volumeMax;
            List<byte> stegoChannel = new List<byte>();

            for (int counter = 0; counter < message.bitsLength; counter++)
            {
                int n = secondCounter * signal.frameRate + sectionCounter * samplesPerSection;
                for (int k = n; k < n + samplesPerSection; k++)
                {
                    stegoChannel.Add((byte)Math.Floor(volume * channel[k] + GetEcho(channel, k, n, counter)));
                }
                sectionCounter++;

                if (sectionCounter == hiddenBitsPerSecond)
                {
                    for (int j = n + samplesPerSection; j < n + samplesPerSection + diff; j++)
                    {
                        stegoChannel.Add((byte)Math.Floor(volume * channel[j] + GetEcho(channel, j, n, counter)));
                    }
                    sectionCounter = 0;
                    secondCounter++;
                }
            }
            signal.CreateAll(stegoChannel, "Шифр");

            return stegoChannel;
        }

        public void CreateStego()
        {
            stegochannels.Add(EmbedStegoMessage(signal.channels[0]));

            if (signal.channelsNum == 2)
            {
                List<byte> segment = signal.channels[1].GetRange(key.Begin, key.End);
                stegochannels.Add(segment);
                signal.CreateAll(segment, "сегмент");
                signal.stego = signal.UniteChannels(stegochannels);
            }
            else
            {
                signal.stego = stegochannels[0];
            }
            signal.CreateAll(signal.stego, "Шифр + Ориг");

            key.Save();
        }
    }


}
