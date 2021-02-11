CREATE VIEW [svc].[uvw_Login]
AS
SELECT UserId,
	UserName,
	AuthTypeId,
	IsPasswordRequired,
	IsPublicKeyRequired,
	EncryptedPass,
	Comment,
	DebugTypeId,
	IsEnabled,
	IsDeletable,
	CreatedUtc
FROM [dbo].[tbl_Login]
