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

            var d = a.GetReferencedAssemblies(); // unused - just exploring.

            Byte[] buffer;

            using (var b = File.OpenRead(Filepath))
            {
                buffer = new Byte[b.Length];
                b.Read(buffer, 0, (int) b.Length);
            }

            AssemblyFile file = new AssemblyFile();

            var s = a.CustomAttributes.Where(t => t.AttributeType == typeof(TargetFrameworkAttribute));

            if (s.Any())
                file.TargetFramework = (string) ((CustomAttributeData)(s.First())).NamedArguments.Where(n => n.MemberName == "FrameworkDisplayName").First().TypedValue.Value;

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

            if (IsPublished(Folderpath))
            {
                var s = File.OpenRead(Pathify(Folderpath, FileName, TargetFramework, Name.Version, FileName));

                if (s.Length != Length)
                    throw new InvalidOperationException("An assembly with these characteristics has already been published that differs from the current assembly.");

                Byte[] buffer = new Byte[s.Length];

                s.Read(buffer, 0, (int)s.Length);

                if (StructuralComparisons.StructuralEqualityComparer.Equals(Contents, buffer) == false)
                    throw new InvalidOperationException("An assembly with these characteristics has already been published that differs from the current assembly.");

                throw new InvalidOperationException("This assembly has already been published.");
            }

            if (Directory.Exists(Folderpath) == false)
                throw new InvalidOperationException("The specified assemblify folder does not exist.");

            if (Directory.Exists(Pathify(Folderpath, FileName)) == false)
                Directory.CreateDirectory(Pathify(Folderpath, FileName));

            if (Directory.Exists(Pathify(Folderpath, FileName, TargetFramework)) == false)
                Directory.CreateDirectory(Pathify(Folderpath, FileName, TargetFramework));

            if (Directory.Exists(Pathify(Folderpath, FileName, TargetFramework, Name.Version)) == false)
                Directory.CreateDirectory(Pathify(Folderpath, FileName, TargetFramework, Name.Version));

            using (var stream = File.Create(Pathify(Folderpath, FileName, TargetFramework, Name.Version, FileName)))
            {
                stream.Write(Contents, 0, Length);
            }

            File.SetAttributes(Pathify(Folderpath, FileName, TargetFramework, Name.Version, FileName), FileAttributes.ReadOnly);
        }

        public bool IsPublished (string Folderpath)
        {
            return (File.Exists(Pathify(Folderpath, FileName, TargetFramework, Name.Version, FileName)));
        }

        /// <summary>
        /// Creates Windows paths from a set of parts.
        /// </summary>
        /// <param name="Parts"></param>
        /// <returns></returns>
        private string Pathify(params object[] Parts)
        {
            if (Parts.Length == 0)
                throw new ArgumentException("The number of parts must be greater than zero.");

            if (Parts.Length == 1)
                return Parts[0].ToString();

            string leader = Parts[0].ToString();

            foreach (object part in Parts.Skip(1))
            {
                leader = leader + @"\" + part.ToString();
            }

            return leader;
        }

        public AssemblyName Name { get; private set; }
        public Byte[] Contents { get; private set; }
        /// <summary>
        /// The version of the CLR that was used when the asssembly was compiled.
        /// </summary>
        public string Runtime { get; private set; }
        public string TargetFramework { get; private set; }
        public int Length { get; private set; }
        public string FileName { get; private set; }
    }
}
