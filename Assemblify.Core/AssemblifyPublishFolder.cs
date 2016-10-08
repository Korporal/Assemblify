using System;

namespace Assemblify.Core
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class AssemblifyPublishFolderAttribute : Attribute
    {
        private string assemblify_folder;

        public AssemblifyPublishFolderAttribute(string AssemblifyFolder)
        {
            assemblify_folder = AssemblifyFolder;
        }

        public string AssemblifyFolder { get { return assemblify_folder; } }
    }
}
