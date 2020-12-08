
CREATE VIEW [svc].[uvw_Login]
AS
SELECT
	UserId
	,UserAuthType
	,UserName
	,FileSystemType
	,FileSystemChrootPath
	,IsPasswordRequired
	,IsPublicKeyRequired
	,IsFileSystemReadOnly
	,Debugger
	,EncryptedPass
	,IsEnabled
	,IsDeletable
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_Login]