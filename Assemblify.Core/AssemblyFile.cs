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
        private static char[] digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        public static bool core_already_loaded = false;
        public static Assembly core = null;

        /// <summary>
        /// Creates an assembly file object by using the assembly name to search for the assembly in the assembly store.
        /// </summary>
        /// <param name="StorePath"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        //public static AssemblyFile CreateFromName(string StorePath, AssemblyName Name)
        //{
        //    if (Directory.Exists(Pathify(StorePath, Name.Name, TargetFramework, Name.Version)) == false)

        //}
        public static AssemblyFile CreateFromFile(string Filepath)
        {

            if (File.Exists(Filepath) == false)
                throw new ArgumentException("The specified asssembly file does not exist.");

            if (core_already_loaded == false)
            {
                // To do reflection on custom attributes when those attributes are defined
                // in another assembly, we must do a reflection-only load on the assembly 
                // that defines the attribute classes, even if that assembly is THIS assembly !
                core = Assembly.ReflectionOnlyLoad(Assembly.GetExecutingAssembly().FullName);
                core_already_loaded = true;
            }

            var assembly = Assembly.ReflectionOnlyLoadFrom(Filepath);  // we have no intention of executing this code.

            Byte[] buffer;

            using (var b = File.OpenRead(Filepath))
            {
                buffer = new Byte[b.Length];
                b.Read(buffer, 0, (int) b.Length);
            }

            AssemblyFile file = new AssemblyFile();

            var targetAttribute = assembly.CustomAttributes.Where(t => t.AttributeType == typeof(TargetFrameworkAttribute));

            if (targetAttribute.Any())
            {
                file.TargetFrameworkName = (string) ((CustomAttributeData)(targetAttribute.First())).NamedArguments.Where(n => n.MemberName == "FrameworkDisplayName").First().TypedValue.Value;
                int first = file.TargetFrameworkName.IndexOfAny(digits);
                file.TargetFramework = Version.Parse(file.TargetFrameworkName.Substring(first).Trim());
            }

            // In assemblify (at this time) we do not support signed assemblies. 
            // Here we consider all referenced assemblies that are not signed.
            // We attach a 'max' framework version that's the target fraamework for the assemby being analyzed.
            // The view being that no referenced assembly can legitimately target a higher version of the framework
            // than the assembly being analyzed (is this always true?)

            var references = assembly.GetReferencedAssemblies();

            file.UnsignedReferences = references.Where(r => r.GetPublicKeyToken().Length == 0).Select(n => new AssemblyData(file.TargetFramework, n)).ToArray();
            file.SignedReferences = references.Where(r => r.GetPublicKeyToken().Length != 0).Select(n => new AssemblyData(file.TargetFramework, n)).ToArray();


            // ReflectionOnlyType was new to me, a bit fiddly but this gives us what we need:
            var folderAttribute = assembly.CustomAttributes.Where(t => t.AttributeType == Type.ReflectionOnlyGetType(typeof(AssemblifyPublishFolderAttribute).AssemblyQualifiedName,true,false));

            if (folderAttribute.Any())
                file.DefaultPublishFolder = (string)((CustomAttributeData)(folderAttribute.First())).ConstructorArguments.First().Value;
            else
                file.DefaultPublishFolder = String.Empty;

            file.Name = assembly.GetName();
            file.Contents = buffer;
            file.Runtime = assembly.ImageRuntimeVersion;
            file.Length = buffer.Length;
            file.FileName = Path.GetFileName(Filepath);
            return file;
        }

        private AssemblyFile()
        {

        }
        public AssemblyData[] SignedReferences { get; private set; }

        public AssemblyData[] UnsignedReferences { get; private set; }
        public bool HasDefaultPublishFolder { get { return DefaultPublishFolder != String.Empty; } }

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

            var p1 = Pathify(Folderpath, FileTitle);

            if (Directory.Exists(p1) == false)
                Directory.CreateDirectory(p1);

            var p2 = Pathify(p1, Name.Version);

            if (Directory.Exists(p2) == false)
                Directory.CreateDirectory(p2);

            var p3 = Pathify(p2, TargetFramework);

            if (Directory.Exists(p3) == false)
                Directory.CreateDirectory(p3);

            using (var stream = File.Create(Pathify(p3, FileName)))
            {
                stream.Write(Contents, 0, Length);
            }

            File.SetAttributes(Pathify(p3, FileName), FileAttributes.ReadOnly);
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


        public AssemblyName Name { get; private set; }
        public Byte[] Contents { get; private set; }
        /// <summary>
        /// The version of the CLR that was used when the asssembly was compiled.
        /// </summary>
        public string Runtime { get; private set; }
        public string TargetFrameworkName { get; private set; }
        public Version TargetFramework { get; private set; }
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

        #region Static Methods and Properties
        /// <summary>
        /// Creates Windows paths from a set of parts.
        /// </summary>
        /// <param name="Parts"></param>
        /// <returns></returns>
        private static string Pathify(params object[] Parts)
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

        // we should have bool TryGetCandidate(AssemblyData Data, string Folderpaath, out AssemblyData Candidate)

        public static bool PublishedCandidateExists (AssemblyData Data, string Folderpath)
        {
            // An exact candidate is a match (obviously on name) on target framework and assembly version.

            return File.Exists(Pathify(Folderpath, Data.AssemblyName.Name, Data.AssemblyName.Version, Data.MaxFramework, Data.AssemblyName.Name + ".dll")); // TODO: Don't simply assume the file itself exists, so revisit this logic..

            // other matches are:
            // same name, lower framwork version, same version
            // same name, lower framework version, lower version
            // same name, lower framework version, higher version
        }

        public static string GetPublishedCandidate (AssemblyData Data, string Folderpath)
        {
            if (PublishedCandidateExists(Data, Folderpath) == false)
                throw new ArgumentException("The specified assembly does not exist as a published assembly.");

            return Pathify(Folderpath, Data.AssemblyName.Name, Data.AssemblyName.Version, Data.MaxFramework, Data.AssemblyName.Name + ".dll");

            // TODO: Consider recursive uses here, ideally taking a single AssemblyFile and returning an array of AssemblyFile[] for each identified candidate DLL file.
        }
        #endregion
    }
}