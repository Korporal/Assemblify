using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;

namespace Assemblify.Core
{
    public sealed class AssemblyFile
    {
        public static AssemblyFile Create(string Filepath)
        {
            if (File.Exists(Filepath) == false)
                throw new ArgumentException("The specified asssembly file does not exist.");

            var a = Assembly.ReflectionOnlyLoadFrom(Filepath);  // we have no intention of executing this code.

            var d = a.GetReferencedAssemblies(); // unused - just exploring.

            Byte[] buffer;

            using (var b = File.OpenRead(Filepath))
            {
                buffer = new Byte[b.Length];
                b.Read(buffer, 0, (int) b.Length);
            }

            AssemblyFile file = new AssemblyFile();

            var targetAttribute = a.CustomAttributes.Where(t => t.AttributeType == typeof(TargetFrameworkAttribute));

            if (targetAttribute.Any())
                file.TargetFramework = (string) ((CustomAttributeData)(targetAttribute.First())).NamedArguments.Where(n => n.MemberName == "FrameworkDisplayName").First().TypedValue.Value;

            // ReflectionOnlyType was new to me, a bit fiddly but this gives us what we need:
            var folderAttribute = a.CustomAttributes.Where(t => t.AttributeType == Type.ReflectionOnlyGetType(typeof(AssemblifyPublishFolderAttribute).AssemblyQualifiedName,true,false));

            if (folderAttribute.Any())
                file.DefaultPublishFolder = (string)((CustomAttributeData)(folderAttribute.First())).ConstructorArguments.First().Value;

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

        public string DefaultPublishFolder { get; private set; }

        public void Publish()
        {
            Publish(DefaultPublishFolder);
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
            if (String.IsNullOrWhiteSpace(Folderpath))
                throw new ArgumentException("The specified publish folder path is invalied.", nameof(Folderpath));
            
            // Firstly, does this assembly already exist in this folder?

            if (IsPublished(Folderpath))
            {
                var s = File.OpenRead(Pathify(Folderpath, FileTitle, TargetFramework, Name.Version, FileName));

                if (s.Length != Length)
                    throw new InvalidOperationException("An assembly with these characteristics has already been published that has a different length to the current assembly.");

                // NOTE: We could create another AssemblyFile instance from the remote file, but if it is the same as 
                // the one we're being asked to publish then this will fail because .Net will not load an assembly more than once.

                Byte[] buffer = new Byte[s.Length];

                s.Read(buffer, 0, (int)s.Length);

                if (StructuralComparisons.StructuralEqualityComparer.Equals(Contents, buffer) == false)
                    throw new InvalidOperationException("An assembly with these characteristics has already been published that has a different binary content to the current assembly.");

                return; // Fine, the assembly's already published and is identical to the one we're trying to publish.
            }

            if (Directory.Exists(Folderpath) == false)
                throw new InvalidOperationException("The specified assemblify folder does not exist.");

            if (Directory.Exists(Pathify(Folderpath, FileTitle)) == false)
                Directory.CreateDirectory(Pathify(Folderpath, FileTitle));

            if (Directory.Exists(Pathify(Folderpath, FileTitle, TargetFramework)) == false)
                Directory.CreateDirectory(Pathify(Folderpath, FileTitle, TargetFramework));

            if (Directory.Exists(Pathify(Folderpath, FileTitle, TargetFramework, Name.Version)) == false)
                Directory.CreateDirectory(Pathify(Folderpath, FileTitle, TargetFramework, Name.Version));

            using (var stream = File.Create(Pathify(Folderpath, FileTitle, TargetFramework, Name.Version, FileName)))
            {
                stream.Write(Contents, 0, Length);
            }

            File.SetAttributes(Pathify(Folderpath, FileTitle, TargetFramework, Name.Version, FileName), FileAttributes.ReadOnly);
        }

        public bool IsPublished()
        {
            return IsPublished(DefaultPublishFolder);
        }

        /// <summary>
        /// Indicates whether this assembly has already been published to the designated folder.
        /// </summary>
        /// <param name="Folderpath"></param>
        /// <returns></returns>
        public bool IsPublished (string Folderpath)
        {
            if (String.IsNullOrWhiteSpace(Folderpath))
                throw new ArgumentException("The specified publish folder path is invalied.", nameof(Folderpath));

            return (File.Exists(Pathify(Folderpath, FileTitle, TargetFramework, Name.Version, FileName)));
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

            StringBuilder leader = new StringBuilder(Parts[0].ToString());

            foreach (object part in Parts.Skip(1))
            {
                leader.Append(@"\" + part.ToString());
            }

            return leader.ToString();
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
        /// <summary>
        /// Returns the assembly's file name without the .dll extension.
        /// </summary>
        public string FileTitle
        {
            get
            {
                return Path.GetFileNameWithoutExtension(FileName);
            }
        }
    }
}
