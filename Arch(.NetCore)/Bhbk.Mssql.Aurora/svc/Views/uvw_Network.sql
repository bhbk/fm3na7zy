
CREATE VIEW svc.uvw_Network
AS
SELECT        Id, IdentityId, Address, Action, Enabled, Created, LastUpdated
FROM            dbo.tbl_Network
