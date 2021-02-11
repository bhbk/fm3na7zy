CREATE VIEW [svc].[uvw_FileSystemType]
AS
SELECT Id,
	Name,
	IsEnabled,
	IsEditable,
	IsDeletable
FROM [dbo].[tbl_FileSystemType]
