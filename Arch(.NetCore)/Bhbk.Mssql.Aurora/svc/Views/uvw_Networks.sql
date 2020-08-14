
CREATE VIEW svc.uvw_Networks
AS
SELECT        Id, UserId, Address, Action, Enabled, Created, LastUpdated
FROM            dbo.tbl_Networks
