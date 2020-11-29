
CREATE VIEW [svc].[uvw_User]
AS
SELECT
	IdentityId
	,IdentityAlias
	,FileSystemType
	,FileSystemChrootPath
	,IsPasswordRequired
	,IsPublicKeyRequired
	,IsFileSystemReadOnly
	,QuotaInBytes
	,QuotaUsedInBytes
	,ConcurrentSessions
	,Debugger
	,IsEnabled
	,IsDeletable
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_User]
