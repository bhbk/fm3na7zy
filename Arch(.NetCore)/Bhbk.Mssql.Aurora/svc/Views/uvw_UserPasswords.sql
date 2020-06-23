
CREATE VIEW [svc].[uvw_UserPasswords]
AS
SELECT        UserId, ConcurrencyStamp, HashPBKDF2, HashSHA256, SecurityStamp, Enabled, Created, Immutable
FROM            dbo.tbl_UserPasswords