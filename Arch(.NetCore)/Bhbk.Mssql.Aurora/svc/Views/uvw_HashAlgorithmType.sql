CREATE VIEW [svc].[uvw_HashAlgorithmType]
AS
SELECT Id,
	Name,
	IsEnabled,
	IsEditable,
	IsDeletable
FROM [dbo].[tbl_HashAlgorithmType]
