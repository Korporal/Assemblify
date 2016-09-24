using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public sealed class AssemblyFile
    {
        public static AssemblyFile Create(string Path)
        {
            if (File.Exists(Path) == false)
                throw new ArgumentException("The specified asssembly file does not exist.");

            var a = Assembly.ReflectionOnlyLoadFrom(Path);
            var b = File.OpenRead(Path);

            Byte[] buffer = new Byte[b.Length];

            b.Read(buffer, 0, (int) b.Length);

            AssemblyFile file = new AssemblyFile();

            var s = a.CustomAttributes.Where(t => t.AttributeType == typeof(TargetFrameworkAttribute));

            if (s.Any())
                file.Target = (string) ((CustomAttributeData)(s.First())).NamedArguments.Where(n => n.MemberName == "FrameworkDisplayName").First().TypedValue.Value;

            file.Name = a.GetName();
            file.Contents = buffer;
            file.Runtime = a.ImageRuntimeVersion;
            file.Length = buffer.Length;
            return file;

        }

        private AssemblyFile()
        {

        }

        public AssemblyName Name { get; private set; }
        public Byte[] Contents { get; private set; }
        /// <summary>
        /// The version of the CLR that was used when the asssembly was compiled.
        /// </summary>
        public string Runtime { get; private set; }
        public string Target { get; private set; }
        public int Length { get; private set; }
    }
}
