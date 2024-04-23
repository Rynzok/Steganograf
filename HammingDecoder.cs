using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steganograf
{
    public class HammingDecoder
    {
        // the decoding matrix
        private string[] H = { "0001111", "0110011", "1010101" };
        private string[] Ht = { "001", "010", "011", "100", "101", "110", "111" };

        private string[] R = { "0010000", "0000100", "0000010", "0000001" };

        public string Decode(string y)
        {
            string z = string.Join("", H.Select(j => Convert.ToString(Convert.ToInt32(j, 2) & Convert.ToInt32(y, 2), 2).Count(c => c == '1') % 2));
            if (Convert.ToInt32(z, 2) > 0)
            {
                int e = Convert.ToInt32(Ht[Convert.ToInt32(z, 2) - 1], 2);
                char[] yArray = y.ToCharArray();
                yArray[e - 1] = yArray[e - 1] == '1' ? '0' : '1';
                y = new string(yArray);
            }

            string x = string.Join("", R.Select(k => Convert.ToString(Convert.ToInt32(k, 2) & Convert.ToInt32(y, 2), 2).Count(c => c == '1') % 2));
            return x;
        }
    }

}
