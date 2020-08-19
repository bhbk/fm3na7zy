
CREATE VIEW [svc].[uvw_UserFolder]
AS
SELECT        Id, IdentityId, ParentId, VirtualName, ReadOnly, Created, LastAccessed, LastUpdated
FROM            dbo.tbl_UserFolder
