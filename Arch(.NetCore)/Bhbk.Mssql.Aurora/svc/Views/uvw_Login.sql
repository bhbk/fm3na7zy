

CREATE VIEW [svc].[uvw_Login]
AS
SELECT
	UserId
	,UserName
	,AuthTypeId
	,FileSystemTypeId
	,FileSystemChrootPath
	,IsPasswordRequired
	,IsPublicKeyRequired
	,IsFileSystemReadOnly
	,DebugTypeId
	,EncryptedPass
	,IsEnabled
	,IsDeletable
	,CreatedUtc

FROM
	[dbo].[tbl_Login]