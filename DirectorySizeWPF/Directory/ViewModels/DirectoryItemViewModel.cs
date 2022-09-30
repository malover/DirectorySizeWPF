using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DirectorySizeWPF
{
    public class DirectoryItemViewModel : BaseViewModel
    {
        public DirectoryItemType Type { get; set; }
        public string FullPath { get; set; }
        public string Name { get { return this.Type == DirectoryItemType.Drive ? this.FullPath : DirectoryStructure.GetFileFolderName(this.FullPath); } }
        public long Size { get; set; }
        public bool isBusy { get; set; }
        public ObservableCollection<DirectoryItemViewModel> Children { get; set; }

        public DirectoryItemViewModel(string fullPath, DirectoryItemType type, long size)
        {
            this.ExpandCommand = new AsyncCommand(Expand, CanExecute);

            this.FullPath = fullPath;
            this.Type = type;
            this.Size = size;

            this.ClearChildren();
        }
        public bool CanExecute()
        {
            return !isBusy;
        }
        public IAsyncCommand ExpandCommand { get; set; }
        public bool CanExpand { get { return this.Type != DirectoryItemType.File; } }
        public bool IsExpanded
        {
            get
            {
                return this.Children?.Count(f => f != null) > 0;
            }
            set
            {
                if (value == true)
                {
                    Expand();
                }
                else
                {
                    this.ClearChildren();
                }
            }
        }

        private async Task Expand()
        {
            isBusy = true;
            if (this.Type == DirectoryItemType.File)
            {
                return;
            }
            List<Task<long>> tasks = new();

            var children = DirectoryStructure.GetDirectoryContents(this.FullPath);
            this.Children = new ObservableCollection<DirectoryItemViewModel>(
                                children.Select(content => new DirectoryItemViewModel(content.FullPath, content.Type, 0)));

            foreach (var item in Children)
            {
                if (item.Type == DirectoryItemType.Folder)
                {
                    new Thread(() =>
                    {
                        DirectoryInfo folder = new DirectoryInfo(item.FullPath);
                        var size = GetDirectorySize(folder);
                        if (size > 0)
                        {
                            item.Size = item.Size + size;
                        }
                    })
                    {
                        IsBackground = true
                    }.Start();
                }
            }
            isBusy = false;
        }
        private long GetDirectorySize(DirectoryInfo root, bool recursive = true)
        {
            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;
            var startDirectorySize = default(long);

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException)
            {
            }

            catch (DirectoryNotFoundException)
            {
            }

            if (files != null)
            {
                //Add size of files in the Current Directory to main size.
                foreach (var fileInfo in files)
                    Interlocked.Add(ref startDirectorySize, fileInfo.Length);
            }
            // Now find all the subdirectories under this directory.

            if (recursive)
            {  //Loop on Sub Direcotries in the Current Directory and Calculate it's files size.
                try
                {
                    subDirs = root.GetDirectories();
                    Parallel.ForEach(subDirs, (subDirectory) =>
                    Interlocked.Add(ref startDirectorySize, GetDirectorySize(subDirectory, recursive)));
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (DirectoryNotFoundException)
                {
                }
            }
            return startDirectorySize;
        }

        private void ClearChildren()
        {
            this.Children = new ObservableCollection<DirectoryItemViewModel>();

            if (this.Type != DirectoryItemType.File)
            {
                this.Children.Add(null);
            }
        }
    }
}
