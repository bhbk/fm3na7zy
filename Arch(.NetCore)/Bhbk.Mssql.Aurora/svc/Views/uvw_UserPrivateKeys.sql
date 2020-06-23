
CREATE VIEW [svc].[uvw_UserPrivateKeys]
AS
SELECT        Id, UserId, PublicKeyId, KeyValueBase64, KeyValueAlgo, KeyValuePass, Enabled, Created, Immutable
FROM            dbo.tbl_UserPrivateKeys