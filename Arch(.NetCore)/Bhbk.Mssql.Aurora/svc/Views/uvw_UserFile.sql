

CREATE   VIEW [svc].[uvw_UserFile]
AS
SELECT
	Id
	,IdentityId
	,FolderId
	,VirtualName
	,IsReadOnly
	,RealPath
	,RealFileName
	,RealFileSize
	,HashSHA256
	,CreatedUtc
	,LastAccessedUtc
	,LastUpdatedUtc
	,LastVerifiedUtc

FROM
	[dbo].[tbl_UserFile]
