using System;
using System.Reflection;

namespace Assemblify.Core
{
    public sealed class AssemblyData
    {
        public AssemblyData(Version MaxFramework, AssemblyName AssemblyName)
        {
            this.MaxFramework = MaxFramework;
            this.AssemblyName = AssemblyName;
        }
        public AssemblyName AssemblyName { get; private set; }
        public Version MaxFramework { get; private set; }

        public override string ToString()
        {
            return AssemblyName.ToString();
        }
    }
}
