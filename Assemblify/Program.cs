using Assemblify.Core;
using System.Reflection;

namespace TestAssemblify
{
    class Program
    {
        static void Main(string[] args)
        {
            bool p;
            AssemblyFile a;

            string assembly_folder = @"C:\assemblify";

            a = AssemblyFile.CreateFromFile(Assembly.GetAssembly(typeof(AssemblyFile)).Location);

            // If this assembly has not yet been published into the designated folder, then publish it.

            if (a.IsPublished() == false)
                a.Publish();

            // Did the whole process actually work?

            p = a.IsPublished();

            // Create an Assemblify AssemblyFile object from a DLL's path:

            a = AssemblyFile.CreateFromFile(@"..\..\..\AssemblyFileInner\bin\Debug\AssemblyFileInner.dll");

            // If this assembly has not yet been published into the designated folder, then publish it.

            if (a.IsPublished() == false)
                a.Publish();

            // Did the whole process actually work?

            p = a.IsPublished();

            var c = AssemblyFile.GetCandidates(a.ReferencedAssemblies[0].AssemblyName, a.TargetFramework, assembly_folder);

            a = AssemblyFile.CreateFromFile(@"..\..\..\AssemblyFileOuter\bin\Debug\AssemblyFileOuter.dll");

            // If this assembly has not yet been published into the designated folder, then publish it.

            if (a.IsPublished() == false)
                a.Publish();

            // Did the whole process actually work?

            p = a.IsPublished();

            AssemblyFileOuter.AssemblyFileOuter s = new AssemblyFileOuter.AssemblyFileOuter(); 

        }
    }
}
