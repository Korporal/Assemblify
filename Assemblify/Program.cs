using Assemblify.Core;

namespace TestAssemblify
{
    class Program
    {
        static void Main(string[] args)
        {
            string assembly_folder = @"E:\assemblify";

            var a = AssemblyFile.Create(@"E:\Git\Repos\Github\assemblify\Assemblify.Core\bin\Debug\Assemblify.Core.dll");

            if (a.IsPublished(assembly_folder) == false)
                a.Publish(assembly_folder);

            bool p = a.IsPublished(assembly_folder);
        }
    }
}
