using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tz_Explorer.ViewController;

namespace tz_Explorer.Model
{
    public class SystemElement: BaseViewModel
    {
        public string Name { get; set; }
        public bool IsExpanded { get; set; }
        public string Path { get; set; }

        private ObservableCollection<SystemElement> _children;
        public ObservableCollection<SystemElement> Children
        {
            get => _children;
            set
            {
                if (_children != value)
                {
                    if (_children != null)
                    {
                        _children.CollectionChanged -= Children_CollectionChanged;
                    }
                    _children = value;
                    if (_children != null)
                    {
                        _children.CollectionChanged += Children_CollectionChanged;
                    }
                }
            }
        }

        public SystemElement(string name, string path)
        {
            Children = new ObservableCollection<SystemElement>();
            Name = name;
            Path = path;
        }
        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Children));
        }

    }
}
