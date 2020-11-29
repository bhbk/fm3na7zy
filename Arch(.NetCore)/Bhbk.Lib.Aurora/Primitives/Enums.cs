using System;

namespace Bhbk.Lib.Aurora.Primitives.Enums
{
    public enum ConfigType
    {
        RebexLicense = 1
    }
    public enum FileSystemProviderType
    {
        Composite = 1,
        Memory = 2,
        SMB = 3,
    }

    public enum JobType
    {
        UnstructuredData,
    }

    public enum NetworkAction
    {
        Allow,
        Deny,
    }

    public enum WorkerType
    {
        WebWorker,
        FtpWorker,
        SftpWorker,
    }
}
