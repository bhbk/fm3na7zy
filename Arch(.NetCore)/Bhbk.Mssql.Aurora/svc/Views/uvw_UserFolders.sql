
CREATE VIEW [svc].[uvw_UserFolders]
AS
SELECT        Id, IdentityId, ParentId, VirtualName, Created, LastAccessed, LastUpdated, ReadOnly
FROM            dbo.tbl_UserFolders
