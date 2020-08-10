using System.DirectoryServices.AccountManagement;
using System.IO;

namespace PSidder
{
    public class UvhdInfo
    {
        public FileInfo File { get; set; }
        public UserPrincipal UserPrincipal { get; set; }
        public bool IsLocked { get; set; }
    }
}
