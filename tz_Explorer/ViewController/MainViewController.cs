using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using tz_Explorer.Extension;
using tz_Explorer.Model;
using tz_Explorer.Service;

namespace tz_Explorer.ViewController
{
    public class MainViewController: BaseViewModel
    {
        public ObservableCollection<SystemElement> SystemElements { get; set; }
        public ICommand SelectDirectoryCommand { get; }
        public ICommand SearchCommand { get; }
        private string _currentDirectory;
        private string _regexToSearch;

        public MainViewController()
        {
            SystemElements = new ObservableCollection<SystemElement>();
            SelectDirectoryCommand = new RelayCommand(SelectDirectory);
            SearchCommand = new RelayCommand(Search);
            CurrentDirectory = SettingsService.GetDefaultDirectoryPath();

        }

        public string CurrentDirectory
        {
            get { return _currentDirectory; }
            set
            {
                _currentDirectory = value;
                OnPropertyChanged(nameof(CurrentDirectory));
            }
        }
        public string RegexToSearch
        {
            get { return _regexToSearch; }
            set
            {
                _regexToSearch = value;
                OnPropertyChanged(nameof(RegexToSearch));
            }
        }
        private void SelectDirectory()
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Выберите директорию";
                folderBrowserDialog.ShowNewFolderButton = false;

                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    CurrentDirectory = folderBrowserDialog.SelectedPath;
                    SettingsService.SaveDefaultDirectoryPath(CurrentDirectory);
                }
            }
        }

        private void Search()
        {
            SystemElements.Clear();
            Task.Run(() => SearchElementsAsync());
        }

        private async Task SearchElementsAsync()
        {
            var regexPattern = new Regex(RegexToSearch, RegexOptions.IgnoreCase);
            await SearchElementsRecursiveAsync(CurrentDirectory, null, regexPattern);
        }

        private async Task SearchElementsRecursiveAsync(string path, SystemElement parent, Regex regex)
        {
            try
            {
                var files = Directory.EnumerateFiles(path);

                foreach (var file in files)
                {
                    if (regex.IsMatch(Path.GetFileName(file)))
                    {
                        var fileItem = new SystemElement(Path.GetFileName(file), file);
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (parent == null)
                            {
                                SystemElements.Add(fileItem);
                            }
                            else
                            {
                                var currentFile = new DirectoryInfo(file);
                                var fileDirectory = new DirectoryInfo(currentFile.Parent.FullName);
                                var previosDirectory = new DirectoryInfo(fileDirectory.Parent.FullName);
                                AddElementToCollection(SystemElementBuilder.BuildHierarchyFromFile(CurrentDirectory, currentFile.FullName));
                            }
                        });
                    }
                }

                var directories = Directory.GetDirectories(path);

                foreach (var directory in directories)
                {
                    var folderItem = new SystemElement(Path.GetFileName(directory), directory);
                    await SearchElementsRecursiveAsync(directory, folderItem, regex);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to path: {path}. {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        private void AddElementToCollection(SystemElement element)
        {

            SystemElements.AddItemToCollection(element);
        }

    }
}
