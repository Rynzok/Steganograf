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
                string binOrd = "";

                //bool flag = false;
                //string left = "", right = "";
                //HammingDecoder code = new HammingDecoder();

                for (int i = 0; i < Bits.Count; i += 16)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        binOrd += Bits[i + j];
                    }
                    for (int k = 0; k < 16; k++)
                    {
                        binOrd += 0;
                    }
                    int bin = Convert.ToInt32(binOrd);
                    char symbol = Convert.ToChar(bin);
                    text += symbol;
                }

                output.WriteLine(text);

                //for (int i = 0; i < BitsLen; i++)
                //{
                //    binOrd += Bits[i].ToString();
                //    counter++;
                //    if (counter == 7)
                //    {
                //        if (flag)
                //        {
                //            right = code.Decode(binOrd);
                //            int symbOrd = Convert.ToInt32(left + right, 2);

                //            string letter;
                //            if (symbOrd == 152 || (0 <= symbOrd && symbOrd <= 2))
                //            {
                //                letter = " ";
                //            }
                //            else
                //            {
                //                byte[] byteOrd = BitConverter.GetBytes(symbOrd);
                //                //letter = System.Text.Encoding.GetEncoding("Windows-1251").GetString(byteOrd)[0];
                //                letter = Encoding.GetEncoding("windows-1251").GetString(byteOrd);
                //            }   

                //            output.Write(letter);

                //            flag = false;
                //            left = right = "";
                //        }
                //        else
                //        {
                //            left = code.Decode(binOrd);
                //            flag = true;
                //        }
                //        binOrd = "";
                //        counter = 0;
                //    }
                //}
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

        public string DecodeSection(List<byte> section)
        {
            List<double> extendedSection = new List<double>();
            int extension = 4;

            foreach (double s in section)
            {
                for (int d = 0; d < extension; d++)
                {
                    extendedSection.Add((double)s - d);
                }
            }

            //Complex[] dft = new Complex[extendedSection.Count];
            //extendedSection.CopyTo(dft);
            //Fourier.Forward(dft);

            Complex[] dft = new Complex[extendedSection.Count];
            for (var i = 0; i < extendedSection.Count; i++)
            {
                dft[i].X = (float)extendedSection[i];
                dft[i].Y = 0;
            }
            FastFourierTransform.FFT(false , 0 , dft);

            List<Complex> sqrLg = new List<Complex>();
            foreach (Complex elem in dft)
            {
                ComplexLog buff = new ComplexLog(elem);
                Complex nbuff = new Complex();
                nbuff.X = (float)buff.comp.Real;
                nbuff.Y = (float)buff.comp.Imaginary;
                sqrLg.Add(nbuff);
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
                //List<double> section = Signal.channels[0].GetRange(counter, SamplesPerSection);
                //var segment = new ArraySegment<double>((double[])Signal.channels[0], counter, counter + SamplesPerSection);
                //List<byte> segment = new List<byte>();
                //for (int i = counter; i < counter + SamplesPerSection; i++)
                //{
                //    segment.Add(Signal.channels[0][i]);
                //}
                List<byte> segment = Signal.channels[0].GetRange(counter, counter + SamplesPerSection);
                //List<double> section = segment.ToList<double>();
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
