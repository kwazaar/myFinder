using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tz_Explorer.Model;

namespace tz_Explorer.Extension
{
    public static class ObservableCollectionExtensions
    {
        public static void AddItemToCollection(this ObservableCollection<SystemElement> collection, SystemElement item)
        {
            if (collection.FirstOrDefault(x => x.Name == item.Name) == null)
            {
                collection.Add(item);
                return;
            }
            foreach (var element in collection)
            {
                if(element.Name == item.Name)
                {
                    if (TryAddItem(element, item.Children.First()))
                    {
                        return;
                    }
                } 
            }
        }

        private static bool TryAddItem(SystemElement parent, SystemElement item)
        {
            foreach (var child in parent.Children)
            {
                if (child.Name == item.Name)
                {
                    if (TryAddItem(child, item.Children.First()))
                    {
                        return true;
                    }
                }
            }
            parent.Children.Add(item);
            return true;
        }

    }
}
