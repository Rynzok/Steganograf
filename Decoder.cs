using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
                string text = "";
                string[] binOrd = new string[Bits.Count / 8];

                for (int i = 0; i < Bits.Count; i++)
                {
                    byte[] buff = new byte[8];
                    int x = 0;
                    for (int j = i * 8; j < i * 8 + 8; j++)
                    {
                        buff[x] = (byte)Bits[j];
                        x++;
                    }
                    binOrd[i] = Encoding.ASCII.GetString(buff);
                    
                }

                foreach (string b in binOrd)
                {
                    text += b;
                }

                output.WriteLine(text);

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
        public Wave Signal { get; set; }
        public BinaryMessageDec Message { get; set; }
        public KeyDec Key { get; set; }

        public int HiddenBitsPerSecond { get; set; }
        public int SamplesPerSection { get; set; }
        public int Diff { get; set; }

        public SystemDec(Wave signal, BinaryMessageDec message, KeyDec key)
        {
            Signal = signal;
            Message = message;
            Key = key;

            HiddenBitsPerSecond = 16;
            SamplesPerSection = Signal.frameRate / HiddenBitsPerSecond;
            Diff = Signal.frameRate % HiddenBitsPerSecond;
        }

        public double GetMod(Complex x)
        {
            return Math.Sqrt(x.X * x.X + x.Y * x.Y);
        }

        public string DecodeSection(byte[] section)
        {
            double[] extendedSection = new double[section.Length];
            int extension = 4;

            foreach (double s in section)
            {
                for (int d = 0; d < extension; d++)
                {
                    extendedSection[d] = (s - d);
                }
            }

            Complex[] dft = new Complex[extendedSection.Length];
            for (var i = 0; i < extendedSection.Length; i++)
            {
                dft[i].X = (float)extendedSection[i];
                dft[i].Y = 0;
            }
            FastFourierTransform.FFT(false , 0 , dft);

            Complex[] sqrLg = new Complex[dft.Length];
            int x = 0;
            foreach (Complex elem in dft)   // проблема. Преобразование длится до 5 секунд
            {
                
                ComplexLog buff = new ComplexLog(elem);
                Complex nbuff = new Complex();
                nbuff.X = (float)buff.comp.Real;
                nbuff.Y = (float)buff.comp.Imaginary;
                sqrLg[x] = nbuff;
                x++;
            }

            Complex[] ift = sqrLg.ToArray();
            FastFourierTransform.FFT(true, 0, ift);

            int i0 = extension * Key.Delta[0], i1 = extension * Key.Delta[1];
            int imax0 = i0, imax1 = i1;

            for (int d = -2; d <= 2; d++)
            {
                if (GetMod(ift[i0 + d]) > GetMod(ift[imax0]))
                {
                    imax0 = i0 + d;
                }
                if (GetMod(ift[i1 + d]) > GetMod(ift[imax1]))
                {
                    imax1 = i1 + d;
                }
            }

            return GetMod(ift[imax0]) > GetMod(ift[imax1]) ? "0" : "1";
        }

        public void ExtractStegomessage()
        {
            int counter = Key.Begin;
            int sectionCounter = 0;

            while (counter < Key.End)
            {
                byte[] segment = Signal.channels[0].GetRange(counter, counter + SamplesPerSection).ToArray();
                Message.Bits.Add(int.Parse(DecodeSection(segment)));

                counter += SamplesPerSection;
                sectionCounter++;

                if (sectionCounter == HiddenBitsPerSecond)
                {
                    counter += Diff;
                    sectionCounter = 0;
                }
            }

            Message.SaveText();
        }
    }
}
