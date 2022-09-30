using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectorySizeWPF
{
    public class DirectoryStructureViewModel : BaseViewModel
    {
        public ObservableCollection<DirectoryItemViewModel> Items { get; set; }

        public DirectoryStructureViewModel()
        {
            var drives = DriveInfo.GetDrives();
            this.Items = new ObservableCollection<DirectoryItemViewModel>(
                drives.Select(drive => new DirectoryItemViewModel(drive.Name, DirectoryItemType.Drive, drive.TotalSize - drive.TotalFreeSpace)));

        }
    }
}
