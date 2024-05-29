using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Steganograf
{
    public class BinaryMessageDec
    {
        public List<int> Bits { get; set; }
        public string OutputTxt { get; set; }

        public BinaryMessageDec()
        {
            Bits = new List<int>();
            OutputTxt = "C:\\Users\\vkise\\OneDrive\\Рабочий стол\\Диплом\\message.txt";
        }

        public void SaveText()
        {
            using (StreamWriter output = new StreamWriter(OutputTxt))
            {
                byte[] binOrd = new byte[Bits.Count / 8];
                string byteOrd = "";


                for (int i = 0; i < Bits.Count / 8; i++)
                {
                    byte[] buff = new byte[8];
                    int x = 0;
                    for (int j = i * 8; j < i * 8 + 8; j++)
                    {
                        buff[x] = (byte)Bits[j];
                        x++;
                    }
                    string b = string.Concat(buff);
                    byte symbol = Convert.ToByte(b, 2);
                    binOrd[i] = symbol;


                }
                Encoding encoding = Encoding.GetEncoding("windows-1251");
                byteOrd = encoding.GetString(binOrd);

                output.WriteLine(byteOrd);

            }
        }
    }

    public class KeyDec
    {
        public List<int> Delta { get; set; }
        public int Begin { get; set; }
        public int End { get; set; }

        public KeyDec(string inputTxt)
        {
            string[] input = File.ReadAllText(inputTxt).Split(' ');
            Delta = new List<int> { int.Parse(input[0]), int.Parse(input[1]) };
            Begin = int.Parse(input[2]);
            End = int.Parse(input[3]);
        }
    }

    public class SystemDec
    {
        public AudioWave Signal { get; set; }
        public BinaryMessageDec Message { get; set; }
        public KeyDec Key { get; set; }

        public int HiddenBitsPerSecond { get; set; }
        public int SamplesPerSection { get; set; }
        public int Diff { get; set; }

        public SystemDec(AudioWave signal, BinaryMessageDec message, KeyDec key)
        {
            Signal = signal;
            Message = message;
            Key = key;

            HiddenBitsPerSecond = 16;
            //SamplesPerSection = Signal.frameRate / HiddenBitsPerSecond;
            SamplesPerSection = 86;
            Diff = Signal.frameRate % HiddenBitsPerSecond;
        }

        public double GetMod(Complex x)
        {
            return Math.Sqrt(x.X * x.X + x.Y * x.Y);
        }

        public string DecodeSection(byte[] section)
        {
            string byteString = "";
            int i = 0;
            int k = 0;
            for (int j = SamplesPerSection; j < section.Length; j += SamplesPerSection)
            {
                if (section[j - 30] == (byte)Math.Abs(section[j] - 30))
                {
                    byteString += "0";
                    i++;
                }
                if (section[j - 40] == (byte)Math.Abs(section[j] - 40))
                {
                    byteString += "1";
                    k++;
                }
            }
            int sum = i + k;

            return byteString;
        }

        public void ExtractStegomessage()
        {
            byte[] segment = new byte[Key.End - Key.Begin];
            Array.Copy(Signal.channels[0], Key.Begin, segment, 0, Key.End - Key.Begin);
            string byteString = DecodeSection(segment);
            foreach (char s in byteString)
            {
                Message.Bits.Add((int)Char.GetNumericValue(s));
            }
        }
    }
}