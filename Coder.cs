using System;
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
        private HammingCoder code;
        public double average;
        public int bitsLength;

        public BinaryMessage(string inputTxt)
        {
            bits = new List<int>();
            input = new StreamReader(inputTxt);
            code = new HammingCoder();

            foreach (char ch in input.ReadToEnd())
            {
                int symbOrd = (int)ch;
                string binOrd = Convert.ToString(symbOrd, 2).PadLeft(8, '0');

                string left = binOrd.Substring(0, 4);
                string encodedLeft = code.Encode(string.Join("", left.Select(c => int.Parse(c.ToString())).ToArray()));
                //bits.AddRange(encodedLeft);
                foreach (string k in encodedLeft.Split())
                {
                    bits.Add(int.Parse(k));
                }

                string right = binOrd.Substring(4);
                string encodedRight = code.Encode(string.Join("", right.Select(c => int.Parse(c.ToString())).ToArray()));
                foreach (string k in encodedRight.Split())
                {
                    bits.Add(int.Parse(k));
                }
            }

            average = bits.Average();
            bitsLength = bits.Count;
        }
    }

    public class Key
    {
        public List<int> delta { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
        public string output_txt { get; set; } = "key.txt";

        public Key()
        {
            delta = new List<int>();
            begin = 0;
            end = 0;
        }

        public void Save()
        {
            using (StreamWriter output = new StreamWriter(output_txt))
            {
                output.Write(string.Join(" ", delta[0], delta[1], begin, end));
            }
        }
    }

    public class System
    {
        private Wave signal;
        private BinaryMessage message;
        private Key key;

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
        private List<int[]> stegochannels;

        public System(Wave signal, BinaryMessage message, Key key)
        {
            this.signal = signal;
            this.message = message;
            this.key = key;

            if (message.average <= 0.5)
            {
                volume0 = volumeMax;
                volume1 = volumeMin;
                key.delta = new List<int> { deltaMax, deltaMin };
            }
            else
            {
                volume0 = volumeMin;
                volume1 = volumeMax;
                key.delta = new List<int> { deltaMin, deltaMax };
            }

            samplesPerSection = signal.frameRate / hiddenBitsPerSecond;
            diff = signal.frameRate % hiddenBitsPerSecond;

            samplesPerMessage = CountSamples();
            key.begin = GetBegin();
            key.end = key.begin + samplesPerMessage;

            stegochannels = new List<int[]>();
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

        private double GetEcho(int[] channel, int k, int n, int counter)
        {
            double echo0 = volume0 * echoVolume * (k >= key.delta[0] ? channel[k - key.delta[0]] : 0) *
                           (1 - SmoothingSignal(counter, k - n));
            double echo1 = volume1 * echoVolume * (k >= key.delta[1] ? channel[k - key.delta[1]] : 0) *
                           SmoothingSignal(counter, k - n);
            return echo0 + echo1;
        }

        private int[] EmbedStegoMessage(int[] channel)
        {
            int secondCounter = key.begin / signal.frameRate;
            int sectionCounter = 0;
            double volume = 1.0 - echoVolume * volumeMax;
            List<int> stegoChannel = new List<int>();

            for (int counter = 0; counter < message.bitsLength; counter++)
            {
                int n = secondCounter * signal.frameRate + sectionCounter * samplesPerSection;
                for (int k = n; k < n + samplesPerSection; k++)
                {
                    stegoChannel.Add((int)Math.Floor(volume * channel[k] + GetEcho(channel, k, n, counter)));
                }
                sectionCounter++;

                if (sectionCounter == hiddenBitsPerSecond)
                {
                    for (int j = n + samplesPerSection; j < n + samplesPerSection + diff; j++)
                    {
                        stegoChannel.Add((int)Math.Floor(volume * channel[j] + GetEcho(channel, j, n, counter)));
                    }
                    sectionCounter = 0;
                    secondCounter++;
                }
            }

            return stegoChannel.ToArray();
        }

        public void CreateStego()
        {
            stegochannels.Add(EmbedStegoMessage((int[])signal.channels[0]));

            if (signal.channelsNum == 2)
            {
                //stegochannels.Add(signal.channels[1].Skip(key.begin).Take(key.end - key.begin).ToArray());
                var segment = new ArraySegment<int> ((int[])signal.channels[1], key.begin, key.end);
                stegochannels.Add(segment.Array);
                signal.stego = signal.UniteChannels(stegochannels);
            }
            else
            {
                signal.stego = stegochannels[0].ToList<int>();
            }

            key.Save();
        }
    }


}
