
CREATE VIEW [svc].[uvw_UserFile]
AS
SELECT        Id, IdentityId, FolderId, VirtualName, ReadOnly, RealPath, RealFileName, RealFileSize, HashSHA256, Created, LastAccessed, LastUpdated, LastVerified
FROM            dbo.tbl_UserFile
