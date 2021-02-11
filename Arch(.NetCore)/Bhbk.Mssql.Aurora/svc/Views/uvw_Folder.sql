CREATE VIEW [svc].[uvw_Folder]
AS
SELECT Id,
	FileSystemId,
	ParentId,
	VirtualName,
	IsReadOnly,
	CreatorId,
	CreatedUtc,
	LastAccessedUtc,
	LastUpdatedUtc
FROM [dbo].[tbl_Folder]
