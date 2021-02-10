
CREATE   VIEW [svc].[uvw_File]
AS
SELECT
	Id
	,UserId
	,FolderId
	,VirtualName
	,IsReadOnly
	,RealPath
	,RealFileName
	,RealFileSize
	,HashTypeId
	,HashValue
	,CreatedUtc
	,LastAccessedUtc
	,LastUpdatedUtc
	,LastVerifiedUtc

FROM
	[dbo].[tbl_File]
