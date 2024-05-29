using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Steganograf
{
    class ImgConteiner
    {
        private Bitmap bitmap;
        private BinaryMessageDec message;
        public int[] binOrd;

        public ImgConteiner(string fileName)
        {
            using (Stream bmpStream = File.Open(fileName, FileMode.Open))
            {
                Image image = Image.FromStream(bmpStream);

                bitmap = new Bitmap(image);

            }
        }

        public void ImageToByte()
        {
            ImageConverter converter = new ImageConverter();
            byte[] bin = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
            binOrd = new int[bin.Length * 8];
            int x = 0;
            for (int i = 0; i < bin.Length; i++)
            {
                int[] b = ByteToBit(bin[i]);
                for (int j = 0; j < b.Length; j++)
                {
                    binOrd[x] = b[j];
                    x++;
                }
            }
        }

        public void CreateImgFromArray(List<int> arr)
        {
            byte[] bytes = new byte[arr.Count / 8];
            int x = 0;
            for (int i = 0; i < arr.Count - 1; i += 8)
            {
                int[] b = new int[8];
                for (int j = 0; j < 8; j++)
                {
                    b[j] = arr[i + j];
                }
                bytes[x] = BitToByte(b);
                x++;
            }
            using (var ms = new MemoryStream(bytes))
            {
                Bitmap bitmap_new = new Bitmap(ms);
                bitmap_new.Save("C:\\Users\\vkise\\OneDrive\\Рабочий стол\\Диплом\\Миска_new.jpg");
            }
        }



        public void EmbedMessage(BinaryMessage message)
        {
            int x = 0;
            int y = 0;
            for (int i = 0; i < message.bitsLength; i += 8)
            {

                //BitArray bitArr = ConvertToBitArray(message.bits);
                Color pColor = bitmap.GetPixel(x, y);
                int[] bitsCurColor = ByteToBit(pColor.R); //бит цветов текущего пикселя
                bitsCurColor[0] = message.bits[i];
                bitsCurColor[1] = message.bits[i + 1];
                byte nR = BitToByte(bitsCurColor); //новый бит цвета пиксея

                bitsCurColor = ByteToBit(pColor.G);//бит цветов текущего пикселя
                bitsCurColor[0] = message.bits[i + 2];
                bitsCurColor[1] = message.bits[i + 3];
                bitsCurColor[2] = message.bits[i + 4];
                byte nG = BitToByte(bitsCurColor);//новый цвет пиксея

                bitsCurColor = ByteToBit(pColor.B);//бит бит цветов текущего пикселя
                bitsCurColor[0] = message.bits[i + 5];
                bitsCurColor[1] = message.bits[i + 6];
                bitsCurColor[2] = message.bits[i + 7];
                byte nB = BitToByte(bitsCurColor);//новый цвет пиксея

                Color nColor = Color.FromArgb(nR, nG, nB); //новый цвет из полученных битов
                bitmap.SetPixel(x, y, nColor); //записали полученный цвет в картинку
                y++;
                if (y == bitmap.Height)
                {
                    x++;
                    y = 0;
                }
            }
        }

        public void ExtractMessage(int count)
        {
            message = new BinaryMessageDec();
            int x = 0;
            int y = 0;
            for (int i = 0; i < count; i += 8)
            {
                Color pColor = bitmap.GetPixel(x, y);
                int[] bitsCurColor = ByteToBit(pColor.R); //бит цветов текущего пикселя
                message.Bits.Add(Convert.ToInt32(bitsCurColor[0]));
                message.Bits.Add(Convert.ToInt32(bitsCurColor[1]));

                bitsCurColor = ByteToBit(pColor.G);//бит цветов текущего пикселя
                message.Bits.Add(Convert.ToInt32(bitsCurColor[0]));
                message.Bits.Add(Convert.ToInt32(bitsCurColor[1]));
                message.Bits.Add(Convert.ToInt32(bitsCurColor[2]));

                bitsCurColor = ByteToBit(pColor.B);//бит бит цветов текущего пикселя
                message.Bits.Add(Convert.ToInt32(bitsCurColor[0]));
                message.Bits.Add(Convert.ToInt32(bitsCurColor[1]));
                message.Bits.Add(Convert.ToInt32(bitsCurColor[2]));
                y++;
                if (y == bitmap.Height)
                {
                    x++;
                    y = 0;
                }
            }

            message.SaveText();
        }

        public void CreateImg()
        {
            bitmap.Save("C:\\Users\\vkise\\OneDrive\\Рабочий стол\\Диплом\\Миска_new.jpg");
        }

        private int[] ByteToBit(byte src)
        {
            int[] bitArray = new int[8];
            for (int i = 0; i < 8; i++)
            {
                if ((src >> i & 1) == 1)
                {
                    bitArray[i] = 1;
                }
                else
                    bitArray[i] = 0;
            }
            return bitArray;
        }

        private byte BitToByte(int[] scr)
        {
            byte num = 0;
            for (int i = 0; i < scr.Length; i++)
                if (scr[i] == 1)
                    num += (byte)Math.Pow(2, i);
            return num;
        }
    }
}
