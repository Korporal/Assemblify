using System;
using System.Collections;
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
        public static AssemblyFile Create(string Filepath)
        {
            if (File.Exists(Filepath) == false)
                throw new ArgumentException("The specified asssembly file does not exist.");

            var a = Assembly.ReflectionOnlyLoadFrom(Filepath);
            var b = File.OpenRead(Filepath);

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
            file.FileName = Path.GetFileName(Filepath);
            return file;

        }

        private AssemblyFile()
        {

        }

        /// <summary>
        /// Create a copy of the assembly in the target folder.
        /// </summary>
        /// <remarks>
        /// The folder must represent an assemblify folder, that is it must be the root folder of an assemblify tree.
        /// </remarks>
        /// <param name="Folderpath"></param>
        public void Publish(string Folderpath)
        {
            // Firstly, does this assembly already exist in this folder?

            if (File.Exists(Folderpath + @"\" + FileName + @"\" + Target + @"\" + Name.Version.ToString() + @"\" + FileName))
            {
                var s = File.OpenRead(Folderpath + @"\" + FileName + @"\" + Target + @"\" + Name.Version.ToString() + @"\" + FileName);

                if (s.Length != Length)
                    throw new InvalidOperationException("An assembly with the characteristics has already been published that differs from the current assembly.");

                Byte[] buffer = new Byte[s.Length];

                s.Read(buffer, 0, (int)s.Length);

                if (StructuralComparisons.StructuralEqualityComparer.Equals(Contents, buffer) == false)
                    throw new InvalidOperationException("An assembly with the characteristics has already been published that differs from the current assembly.");

                throw new InvalidOperationException("This assembly has already been published.");
            }


            if (Directory.Exists(Folderpath) == false)
                throw new InvalidOperationException("The specified assemblify folder does not exist.");

            if (Directory.Exists(Folderpath + @"\" + FileName) == false)
                Directory.CreateDirectory(Folderpath + @"\" + FileName);

            if (Directory.Exists(Folderpath + @"\" + FileName + @"\" + Target) == false)
                Directory.CreateDirectory(Folderpath + @"\" + FileName + @"\" + Target);

            if (Directory.Exists(Folderpath + @"\" + FileName + @"\" + Target + @"\" + Name.Version.ToString()) == false)
                Directory.CreateDirectory(Folderpath + @"\" + FileName + @"\" + Target + @"\" + Name.Version.ToString());

            using (var stream = File.Create(Folderpath + @"\" + FileName + @"\" + Target + @"\" + Name.Version.ToString() + @"\" + FileName))
            {
                stream.Write(Contents, 0, Length);
            }
        }

        public AssemblyName Name { get; private set; }
        public Byte[] Contents { get; private set; }
        /// <summary>
        /// The version of the CLR that was used when the asssembly was compiled.
        /// </summary>
        public string Runtime { get; private set; }
        public string Target { get; private set; }
        public int Length { get; private set; }
        public string FileName { get; private set; }
    }
}
