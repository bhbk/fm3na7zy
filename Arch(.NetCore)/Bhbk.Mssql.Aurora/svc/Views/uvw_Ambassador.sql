CREATE VIEW [svc].[uvw_Ambassador]
AS
SELECT Id,
	UserPrincipalName,
	EncryptedPass,
	IsEnabled,
	IsDeletable,
	CreatedUtc
FROM [dbo].[tbl_Ambassador]
