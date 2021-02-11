CREATE VIEW [svc].[uvw_PublicKey]
AS
SELECT Id,
	UserId,
	PrivateKeyId,
	KeyValue,
	KeyAlgorithmId,
	KeyFormatId,
	SigValue,
	SigAlgorithmId,
	Comment,
	IsEnabled,
	IsDeletable,
	CreatedUtc
FROM [dbo].[tbl_PublicKey]
