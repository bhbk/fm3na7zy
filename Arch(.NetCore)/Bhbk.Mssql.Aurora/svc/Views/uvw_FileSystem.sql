CREATE VIEW [svc].[uvw_FileSystem]
AS
SELECT Id,
	FileSystemTypeId,
	Name,
	Description,
	UncPath,
	CreatedUtc,
	IsEnabled,
	IsDeletable
FROM [dbo].[tbl_FileSystem]
