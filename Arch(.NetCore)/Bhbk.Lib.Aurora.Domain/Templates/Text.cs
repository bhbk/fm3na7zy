using System;

namespace Bhbk.Lib.Aurora.Domain.Templates
{
    public class Text
    {
        public static string NotifyOnFileDelete(string server, string userName, string firstName, string lastName, string filePath)
        {
            return $"Hi {string.Format("{0} {1}", firstName, lastName)}. The user {userName} accessed {server} via SFTP. " + Environment.NewLine
                + Environment.NewLine + $"The file {filePath} was deleted. ";
        }

        public static string NotifyOnFileGetContent(string server, string userName, string firstName, string lastName, string filePath, string byteSize, string client)
        {
            return $"Hi {string.Format("{0} {1}", firstName, lastName)}. The user {userName} accessed {server} via SFTP. " + Environment.NewLine
                + Environment.NewLine + $"The file {filePath} downloaded to {client} is {byteSize} bytes. ";
        }

        public static string NotifyOnFileSaveContent(string server, string userName, string firstName, string lastName, string filePath, string bytesSize, string client)
        {
            return $"Hi {string.Format("{0} {1}", firstName, lastName)}. The user {userName} accessed {server} via SFTP. " + Environment.NewLine
                + Environment.NewLine + $"The file {filePath} uploaded from {client} is {bytesSize} bytes. " + Environment.NewLine
                + Environment.NewLine + $"The data within will be processed without any more humans or files involved now. ";
        }
    }
}
