
CREATE VIEW [svc].[uvw_Network]
AS
SELECT        Id, IdentityId, Address, Action, IsEnabled, CreatedUtc, LastUpdatedUtc
FROM            [dbo].[tbl_Network]
