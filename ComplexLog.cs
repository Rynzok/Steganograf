using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Steganograf
{
    internal class ComplexLog
    {
        public Complex comp;
        public ComplexLog(NAudio.Dsp.Complex elum)
        {
            comp = new Complex(elum.X, elum.Y);
            comp = Complex.Log(comp);
            comp = Complex.Pow(comp, 2);
        }
    }
}
