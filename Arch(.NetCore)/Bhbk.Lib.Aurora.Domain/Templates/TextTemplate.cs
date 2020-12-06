using System;

namespace Bhbk.Lib.Aurora.Domain.Templates
{
    public class TextTemplate
    {
        public static string NotifyOnFileDelete(string server, string userName, string displayName, string filePath)
        {
            return $"Hi {displayName}. The user {userName} accessed {server} via SFTP. " + Environment.NewLine
                + Environment.NewLine + $"The file {filePath} was deleted. ";
        }

        public static string NotifyOnFileDownload(string server, string userName, string displayName, string filePath, string byteSize, string client)
        {
            return $"Hi {displayName}. The user {userName} accessed {server} via SFTP. " + Environment.NewLine
                + Environment.NewLine + $"The file {filePath} downloaded to {client} is {byteSize} bytes. ";
        }

        public static string NotifyOnFileUpload(string server, string userName, string displayName, string filePath, string bytesSize, string client)
        {
            return $"Hi {displayName}. The user {userName} accessed {server} via SFTP. " + Environment.NewLine
                + Environment.NewLine + $"The file {filePath} uploaded from {client} is {bytesSize} bytes. " + Environment.NewLine
                + Environment.NewLine + $"The data within will be processed without any more humans or files involved now. ";
        }
    }
}
