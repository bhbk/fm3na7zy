CREATE VIEW [svc].[uvw_KeyAlgorithmType]
AS
SELECT Id,
	Name,
	IsEnabled,
	IsEditable,
	IsDeletable
FROM [dbo].[tbl_KeyAlgorithmType]
