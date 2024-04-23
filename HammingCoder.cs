using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steganograf
{
    public class HammingCoder
    {
        // the encoding matrix
        private static readonly string[] G = { "1101", "1011", "1000", "0111", "0100", "0010", "0001" };

        public string Encode(string x)
        {
            var y = string.Concat(G.Select(g => (Convert.ToInt32(g, 2) & Convert.ToInt32(x, 2)).ToString("D1").Count(c => c == '1') % 2));
            return y;
        }
    }

}
