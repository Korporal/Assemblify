using Assemblify.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestAssemblify
{
    class Program
    {
        static void Main(string[] args)
        {
            string assembly_folder = @"E:\assemblify";

            var a = AssemblyFile.Create(@"E:\Git\Repos\Github\assemblify\Core\bin\Debug\Assemblify.Core.dll");

            if (a.IsPublished(assembly_folder) == false)
                a.Publish(assembly_folder);

            bool p = a.IsPublished(assembly_folder);
        }
    }
}
