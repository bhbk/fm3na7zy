CREATE VIEW [svc].[uvw_PrivateKey]
AS
SELECT Id,
	UserId,
	PublicKeyId,
	KeyValue,
	KeyAlgorithmId,
	KeyFormatId,
	EncryptedPass,
	IsEnabled,
	IsDeletable,
	CreatedUtc
FROM [dbo].[tbl_PrivateKey]
