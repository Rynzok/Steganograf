using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

            char[] symbolsArray = new char[inputText.Length];
            for (int i = 0; i < inputText.Length; i++)
            {
                symbolsArray[i] = inputText[i];
            }
            byte[] symbol = Encoding.GetEncoding(1251).GetBytes(symbolsArray);
            foreach (byte s in symbol)
            {
                string binary = Convert.ToString(s, 2).PadLeft(8, '0');

                for (int i = 0; i < binary.Length; i++)
                {
                    bits.Add((int)Char.GetNumericValue(binary[i]));
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
        public AudioWave signal;
        private int[] message;
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

        private int samplesPerMessage;

        public Systema(AudioWave signal, int[] message, Key key)
        {
            this.signal = signal;
            this.message = message;
            this.key = key;

            if (message.Average() <= 0.5)
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

            samplesPerMessage = CountSamples();
            key.Begin = GetBegin();
            key.End = key.Begin + samplesPerMessage;
        }

        private int CountSamples()
        {
            int divPart = message.Length * 86 + 86;
            return divPart;
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


        public void CreateStego()
        {

            byte[] coderSignal = new byte[key.End - key.Begin];
            Array.Copy(signal.channels[0], key.Begin, coderSignal, 0, key.End - key.Begin);

            signal.stego = EmdebStegoMes(coderSignal);

            Array.Copy(signal.stego, 0, signal.channels[0], key.Begin, key.End - key.Begin);

            key.Save();
        }

        private byte[] EmdebStegoMes(byte[] content)
        {
            int i = 0;
            byte[] stego = new byte[content.Length];
            for (int j = 0; j < stego.Length; j++)
            {
                stego[j] = content[j];
                if (j % 86 == 0 && j != 0)
                {
                    if (message[i] == 0)
                    {
                        stego[j - 30] = (byte)Math.Abs(stego[j] - 30);
                        if (stego[j - 40] == (byte)Math.Abs(stego[j] - 40))
                        {
                            stego[j - 40]--;
                        }
                    }
                    else
                    {
                        stego[j - 40] = (byte)Math.Abs(stego[j] - 40);
                        if (stego[j - 30] == (byte)Math.Abs(stego[j] - 30))
                        {
                            stego[j - 30]--;
                        }
                    }
                    i++;
                }
            }
            return stego;
        }
    }

}