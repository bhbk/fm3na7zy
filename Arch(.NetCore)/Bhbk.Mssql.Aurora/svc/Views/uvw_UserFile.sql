
CREATE VIEW [svc].[uvw_UserFile]
AS
SELECT        Id, IdentityId, FolderId, VirtualName, RealPath, RealFileName, RealFileSize, HashSHA256, IsReadOnly, CreatedUtc, LastAccessedUtc, LastUpdatedUtc, LastVerifiedUtc
FROM            [dbo].[tbl_UserFile]
