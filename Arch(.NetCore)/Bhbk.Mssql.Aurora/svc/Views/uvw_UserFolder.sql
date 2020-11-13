
CREATE VIEW [svc].[uvw_UserFolder]
AS
SELECT        Id, IdentityId, ParentId, VirtualName, IsReadOnly, CreatedUtc, LastAccessedUtc, LastUpdatedUtc
FROM            [dbo].[tbl_UserFolder]
