using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.DirectoryServices.AccountManagement;
using System.IO;

namespace PSidder
{
    [Cmdlet(VerbsCommon.Get, "UserProfileDisk")]
    [OutputType(nameof(UvhdInfo))]
    public class PSidder : PSCmdlet
    {
        private readonly PrincipalContext principalContext = new PrincipalContext(ContextType.Domain);

        [Parameter(
            Mandatory = false,
            Position = 0,
            HelpMessage = "Specifies a path to one or more locations in which to search user profile disks"
            )]
        public HashSet<string> Path { get; set; } = null;

        [Parameter(
            Mandatory = false,
            HelpMessage = "Return only locked profile disks")]
        public SwitchParameter Locked { get; set; }

        protected override void BeginProcessing()
        {
            if (Path.Count == 1 && string.IsNullOrWhiteSpace(Path.First())) {
                Path.Clear();
            }

            if (Path.Count < 1)
            {
                Path.Add(SessionState.Path.CurrentFileSystemLocation.ProviderPath);
            }
        }

        protected override void ProcessRecord()
        {
            var disks = Path
                .Select(p => GetProfileDisks(p))
                .Aggregate(Enumerable.Empty<UvhdInfo>(), (acc, ppp) => acc.Concat(ppp))
                .ToList();

            WriteObject(disks
                .Where(d => !Locked.ToBool() || d.IsLocked)
                .ToList());
        }

        private IEnumerable<UvhdInfo> GetProfileDisks(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            if (!directory.Exists)
            {
                throw new FileNotFoundException(path);
            }

            var files = directory.GetFiles("UVHD-*.vhdx");

            foreach (var file in files)
            {
                string sid = GetSidFromFilename(file.Name);
                UserPrincipal userPrincipal = TryGetUserPrincipalFromSid(sid);

                bool locked = FileIsLocked(file);

                yield return new UvhdInfo
                {
                    File = file,
                    IsLocked = locked,
                    UserPrincipal= userPrincipal,
                };
            }
        }

        private string GetSidFromFilename(string filename)
        {
            if (!filename.StartsWith("UVHD-", StringComparison.OrdinalIgnoreCase) || !filename.EndsWith(".VHDX", StringComparison.OrdinalIgnoreCase)) {
                // File is not a user profile disk
                return null;
            }

            return filename.Substring(5, filename.Length - 10);
        }

        private UserPrincipal TryGetUserPrincipalFromSid(string sid)
        {
            try
            {
                return UserPrincipal.FindByIdentity(principalContext, sid);
            }
            catch
            {
                return null;
            }
        }

        private bool FileIsLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new Exception("You don't have sufficient rights to check if the user profile disks are locked or not," +
                    "you need full control in both smb share permissions and ntfs acls", e);
            }
            catch
            {
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            return false;
        }
    }
}
