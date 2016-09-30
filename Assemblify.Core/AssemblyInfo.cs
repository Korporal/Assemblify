using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assemblify.Core
{
    public sealed class AssemblyInfo
    {
        public AssemblyInfo(string MaxFramework, AssemblyName AssemblyName)
        {
            this.MaxFramework = MaxFramework;
            this.AssemblyName = AssemblyName;
        }
        public AssemblyName AssemblyName { get; private set; }
        public string MaxFramework { get; private set; }
    }
}
