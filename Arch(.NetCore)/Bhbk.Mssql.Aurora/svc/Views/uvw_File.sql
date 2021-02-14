
CREATE VIEW [svc].[uvw_File]
AS
SELECT Id,
	FileSystemId,
	FolderId,
	VirtualName,
	IsReadOnly,
	RealPath,
	RealFileName,
	RealFileSize,
	HashTypeId,
	HashValue,
	CreatorId,
	CreatedUtc,
	LastAccessedUtc,
	LastUpdatedUtc,
	LastVerifiedUtc
FROM [dbo].[tbl_File]
