using System;

namespace Bhbk.Lib.Aurora.Domain.Primitives.Enums
{
    public enum GroupType
    {
        Daemons,
        Users,
    }

    public enum JobType
    {
        MOTDDownloadJob,
        MOTDUploadJob,
        UnstructuredData,
    }
}
