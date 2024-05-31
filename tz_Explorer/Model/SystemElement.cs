using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using tz_Explorer.ViewController;
using tz_Explorer.Service;

namespace tz_Explorer.Model
{
    public class SystemElement: BaseViewModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public ImageSource Icon { get; set; }
        public ObservableCollection<SystemElement> Children { get; set; }
        public SystemElement(string name, string path)
        {
            Children = new ObservableCollection<SystemElement>();
            Name = name;
            Path = path;
        }
        public SystemElement(string name, string path, bool isFolder)
        {
            Children = new ObservableCollection<SystemElement>();
            Name = name;
            Path = path;
            Icon = IconService.GetIcon(path, isFolder);
        }


    }
}
