using System;

namespace Bhbk.Lib.Aurora.Domain.Primitives.Enums
{
    public enum NetworkAction
    {
        Allow,
        Deny,
    }

    public enum JobType
    {
        UnstructuredData,
    }

    public enum WorkerType
    {
        AdminWorker,
        FtpWorker,
        SftpWorker,
    }
}
