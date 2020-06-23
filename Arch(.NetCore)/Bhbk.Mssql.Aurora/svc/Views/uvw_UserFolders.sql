
CREATE VIEW [svc].[uvw_UserFolders]
AS
SELECT        Id, UserId, ParentId, VirtualName, Created, LastAccessed, LastUpdated, ReadOnly
FROM            dbo.tbl_UserFolders