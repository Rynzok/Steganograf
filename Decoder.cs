using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace Steganograf
{
    public class BinaryMessageDec
    {
        public List<int> Bits { get; set; }
        public int BitsLen { get; set; }
        public string OutputTxt { get; set; }

        public BinaryMessageDec()
        {
            Bits = new List<int>();
            BitsLen = 0;
            OutputTxt = "message.txt";
        }

        public void SetBitsLen()
        {
            BitsLen = Bits.Count;
        }

        public void SaveText()
        {
            using (StreamWriter output = new StreamWriter(OutputTxt))
            {
                int counter = 0;
                string binOrd = "";

                bool flag = false;
                string left = "", right = "";
                HammingDecoder code = new HammingDecoder();

                SetBitsLen();

                for (int i = 0; i < BitsLen; i++)
                {
                    binOrd += Bits[i].ToString();
                    counter++;
                    if (counter == 7)
                    {
                        if (flag)
                        {
                            right = code.Decode(binOrd);
                            int symbOrd = Convert.ToInt32(left + right, 2);

                            string letter;
                            if (symbOrd == 152 || (0 <= symbOrd && symbOrd <= 2))
                            {
                                letter = " ";
                            }
                            else
                            {
                                byte[] byteOrd = BitConverter.GetBytes(symbOrd);
                                //letter = System.Text.Encoding.GetEncoding("Windows-1251").GetString(byteOrd)[0];
                                letter = Encoding.GetEncoding("windows-1251").GetString(byteOrd);
                            }   

                            output.Write(letter);

                            flag = false;
                            left = right = "";
                        }
                        else
                        {
                            left = code.Decode(binOrd);
                            flag = true;
                        }
                        binOrd = "";
                        counter = 0;
                    }
                }
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
        public Key Key { get; set; }

        public int HiddenBitsPerSecond { get; set; }
        public int SamplesPerSection { get; set; }
        public int Diff { get; set; }

        public SystemDec(Wave signal, BinaryMessageDec message, Key key)
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

        public string DecodeSection(List<double> section)
        {
            List<double> extendedSection = new List<double>();
            int extension = 4;

            foreach (double s in section)
            {
                for (int d = 0; d < extension; d++)
                {
                    extendedSection.Add(s - d);
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
            FastFourierTransform.FFT(true , 0 , dft);

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
            FastFourierTransform.FFT(false, 0, ift);

            int i0 = extension * Key.delta[0], i1 = extension * Key.delta[1];
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
            int counter = Key.begin;
            int sectionCounter = 0;

            while (counter < Key.end)
            {
                //List<double> section = Signal.channels[0].GetRange(counter, SamplesPerSection);
                var segment = new ArraySegment<double>((double[])Signal.channels[0], counter, counter + SamplesPerSection);
                List<double> section = segment.ToList();
                Message.Bits.Add(int.Parse(DecodeSection(section)));

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
