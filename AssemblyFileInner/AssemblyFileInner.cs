using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;

namespace AssemblyFileInner
{
    public class AssemblyFileInner
    {
        BinaryFormatter formatter; // NOTE: This class acutally sits inside mscorlib !!
        Complex complex;
        public AssemblyFileInner()
        {
            formatter = new BinaryFormatter();
            complex = new Complex(12, 6);
        }
    }
}
