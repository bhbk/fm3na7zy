CREATE VIEW [svc].[uvw_PublicKeySignatureType]
AS
SELECT Id,
	Name,
	IsEnabled,
	IsEditable,
	IsDeletable
FROM [dbo].[tbl_PublicKeySignatureType]
