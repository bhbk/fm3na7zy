
CREATE VIEW svc.uvw_Networks
AS
SELECT        Id, IdentityId, Address, Action, Enabled, Created, LastUpdated
FROM            dbo.tbl_Networks
