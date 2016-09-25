using Assemblify.Core;

namespace TestAssemblify
{
    class Program
    {
        static void Main(string[] args)
        {
            string assembly_folder = @"E:\assemblify";

            // Create an Assemblify AssemblyFile object from a DLL's path:

            var a = AssemblyFile.Create(@"E:\Git\Repos\Github\assemblify\Assemblify.Core\bin\Debug\Assemblify.Core.dll");

            // If this assembly has not yet been published into the designated folder, then publish it.

            if (a.IsPublished() == false)
                a.Publish();

            // Did the whole process actually work?

            bool p = a.IsPublished();
        }
    }
}
