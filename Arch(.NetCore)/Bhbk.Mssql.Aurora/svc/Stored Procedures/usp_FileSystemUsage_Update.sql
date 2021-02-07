
CREATE PROCEDURE [svc].[usp_FileSystemUsage_Update]
     @FileSystemId uniqueidentifier
	,@QuotaInBytes bigint
	,@QuotaUsedInBytes bigint

AS

SET NOCOUNT ON;

UPDATE [dbo].[tbl_FileSystemUsage]
SET QuotaInBytes = @QuotaInBytes
   ,QuotaUsedInBytes = @QuotaUsedInBytes
WHERE FileSystemId = @FileSystemId

SELECT * 
FROM [dbo].[tbl_FileSystemUsage] 
WHERE FileSystemId = @FileSystemId
