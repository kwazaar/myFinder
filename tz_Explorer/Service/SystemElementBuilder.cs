using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tz_Explorer.Model;

namespace tz_Explorer.Service
{
    public class SystemElementBuilder
    {
        public static SystemElement BuildHierarchyFromFile(string rootFolderPath, string filePath)
        {
            var rootDirectory = new DirectoryInfo(rootFolderPath);
            var fileElement = new SystemElement(Path.GetFileName(filePath), filePath, false);

            return BuildHierarchy(rootDirectory, fileElement);
        }

        private static SystemElement BuildHierarchy(DirectoryInfo rootDirectory, SystemElement currentElement)
        {
            var currentDirectory = new DirectoryInfo(currentElement.Path);

            if (currentDirectory.Parent.FullName.Equals(rootDirectory.FullName, StringComparison.OrdinalIgnoreCase))
            {
                return currentElement;
            }

            var parentDirectory = currentDirectory.Parent;
            if (parentDirectory == null)
            {
                return currentElement;
            }

            var parentElement = new SystemElement(parentDirectory.Name, parentDirectory.FullName, true);
            parentElement.Children.Add(currentElement);

            return BuildHierarchy(rootDirectory, parentElement);
        }
    }
}
