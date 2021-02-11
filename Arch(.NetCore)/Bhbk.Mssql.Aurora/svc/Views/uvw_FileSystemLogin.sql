CREATE VIEW [svc].[uvw_FileSystemLogin]
AS
SELECT FileSystemId,
	UserId,
	SmbAuthTypeId,
	AmbassadorId,
	ChrootPath,
	CreatedUtc,
	IsReadOnly
FROM [dbo].[tbl_FileSystemLogin]
