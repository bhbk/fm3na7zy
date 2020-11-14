using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Authorization;
using FubarDev.FtpServer.BackgroundTransfer;
using FubarDev.FtpServer.CommandExtensions;
using FubarDev.FtpServer.CommandHandlers;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.ConnectionHandlers;
using FubarDev.FtpServer.DataConnection;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters;
using FubarDev.FtpServer.Localization;
using FubarDev.FtpServer.ServerCommandHandlers;
using FubarDev.FtpServer.ServerCommands;
using FubarDev.FtpServer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Bhbk.Daemon.Aurora.FTP.FileSystems
{
    public class CompositeReadWriteFileSystem : IUnixFileSystem
    {
        public bool SupportsAppend => throw new NotImplementedException();

        public bool SupportsNonEmptyDirectoryDelete => throw new NotImplementedException();

        public StringComparer FileSystemEntryComparer => throw new NotImplementedException();

        public IUnixDirectoryEntry Root => throw new NotImplementedException();

        public CompositeReadWriteFileSystem()
        {

        }

        public Task<IBackgroundTransfer> AppendAsync(IUnixFileEntry fileEntry, long? startPosition, Stream data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IBackgroundTransfer> CreateAsync(IUnixDirectoryEntry targetDirectory, string fileName, Stream data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IUnixDirectoryEntry> CreateDirectoryAsync(IUnixDirectoryEntry targetDirectory, string directoryName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync(IUnixDirectoryEntry directoryEntry, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IUnixFileSystemEntry> GetEntryByNameAsync(IUnixDirectoryEntry directoryEntry, string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IUnixFileSystemEntry> MoveAsync(IUnixDirectoryEntry parent, IUnixFileSystemEntry source, IUnixDirectoryEntry target, string fileName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> OpenReadAsync(IUnixFileEntry fileEntry, long startPosition, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IBackgroundTransfer> ReplaceAsync(IUnixFileEntry fileEntry, Stream data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IUnixFileSystemEntry> SetMacTimeAsync(IUnixFileSystemEntry entry, DateTimeOffset? modify, DateTimeOffset? access, DateTimeOffset? create, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
