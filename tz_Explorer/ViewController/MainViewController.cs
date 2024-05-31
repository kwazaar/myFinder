using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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
        public ICommand PauseResumeCommand { get; }

        private CancellationTokenSource _cancellationTokenSource;
        private System.Timers.Timer _timer;
        private DateTime _startTime;
        private TimeSpan _pausedTime;
        private DateTime _pauseStartTime;

        public MainViewController()
        {
            SystemElements = new ObservableCollection<SystemElement>();
            ElapsedTime = "00:00:00";

            SelectDirectoryCommand = new RelayCommand(SelectDirectory);
            SearchCommand = new RelayCommand(Search);
            PauseResumeCommand = new RelayCommand(PauseResume);
            CurrentDirectory = SettingsService.GetDefaultDirectoryPath();
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
            if (!SearchStart)
            {
                SystemElements.Clear();
                Task.Run(() => SearchElementsAsync());
            }
            else
            {
                _cancellationTokenSource.Cancel();
            }
        }

        private async Task SearchElementsAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var regexPattern = new Regex(RegexToSearch, RegexOptions.IgnoreCase);
            
            StartSearch();
            await SearchElementsRecursiveAsync(CurrentDirectory, null, regexPattern, _cancellationTokenSource.Token);
            StopSearch();
        }

        private async Task SearchElementsRecursiveAsync(string path, SystemElement parent, Regex regex, CancellationToken cancellationToken)
        {
            try
            {
                var directories = Directory.GetDirectories(path);

                foreach (var directory in directories)
                {
                    await PauseIfNeededOrCancelSearch(cancellationToken);
                    
                    var folderItem = new SystemElement(Path.GetFileName(directory), directory, true);
                    await SearchElementsRecursiveAsync(directory, folderItem, regex, cancellationToken);
                }

                var files = Directory.EnumerateFiles(path);

                foreach (var file in files)
                {
                    await PauseIfNeededOrCancelSearch(cancellationToken);

                    FileChecked++;

                    if (regex.IsMatch(Path.GetFileName(file)))
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            var fileItem = new SystemElement(Path.GetFileName(file), file, false);
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
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Нет доступа по адресу: {path}. {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Поиск отменен");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private async Task PauseIfNeededOrCancelSearch(CancellationToken cancellationToken)
        {
            while (_isPaused)
            {
                await Task.Delay(100, cancellationToken); 
            }
            cancellationToken.ThrowIfCancellationRequested();
        }

        private void PauseResume()
        {
            IsPaused = !IsPaused;
            if(IsPaused)
                _pauseStartTime = DateTime.Now;
            else
                _pausedTime += DateTime.Now - _pauseStartTime;
        }

        private void StartSearch()
        {
            SearchStart = true;
            FileChecked = 0;
            ElapsedTime = "00:00:00";
            _startTime = DateTime.Now;
            _pausedTime = TimeSpan.Zero;
            _timer = new System.Timers.Timer(1000); 
            _timer.Elapsed += UpdateElapsedTime;
            _timer.Start();
        }

        private void StopSearch()
        {
            if (_timer != null)
            {
                SearchStart = false;
                _timer.Stop();
                _timer.Elapsed -= UpdateElapsedTime;
                _timer.Dispose();
                _timer = null;
                IsPaused = false;
            }
        }

        private void UpdateElapsedTime(object sender, ElapsedEventArgs e)
        {
            if (!IsPaused)
            {
                var elapsed = DateTime.Now - _startTime - _pausedTime;
                ElapsedTime = elapsed.ToString(@"hh\:mm\:ss");
                OnPropertyChanged(nameof(ElapsedTime));
            }
        }

        private void AddElementToCollection(SystemElement element)
        {
            SystemElements.AddItemToCollection(element);
        }

        private string _currentDirectory;
        public string CurrentDirectory
        {
            get { return _currentDirectory; }
            set
            {
                _currentDirectory = value;
                OnPropertyChanged(nameof(CurrentDirectory));
            }
        }

        private string _regexToSearch;
        public string RegexToSearch
        {
            get { return _regexToSearch; }
            set
            {
                _regexToSearch = value;
                OnPropertyChanged(nameof(RegexToSearch));
            }
        }

        private int _filesChecked;
        public int FileChecked
        {
            get { return _filesChecked; }
            set
            {
                _filesChecked = value;
                OnPropertyChanged(nameof(FileChecked));
            }
        }
        private string _elapsedTime;
        public string ElapsedTime
        {
            get { return _elapsedTime; }
            set
            {
                _elapsedTime = value;
                OnPropertyChanged(nameof(ElapsedTime));
            }
        }
        private bool _searchStart;
        public bool SearchStart
        {
            get { return _searchStart; }
            set
            {
                _searchStart = value;
                OnPropertyChanged(nameof(SearchStart));
            }
        }
        private bool _isPaused;
        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                _isPaused = value;
                OnPropertyChanged(nameof(IsPaused));
            }
        }

    }
}
