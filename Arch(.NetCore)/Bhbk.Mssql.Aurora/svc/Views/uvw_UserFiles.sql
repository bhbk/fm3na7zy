
CREATE VIEW [svc].[uvw_UserFiles]
AS
SELECT        Id, UserId, FolderId, VirtualName, ReadOnly, RealPath, RealFileName, RealFileSize, HashSHA256, Created, LastAccessed, LastUpdated, LastVerified
FROM            dbo.tbl_UserFiles
