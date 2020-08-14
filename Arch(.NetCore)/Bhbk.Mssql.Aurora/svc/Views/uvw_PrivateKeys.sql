
CREATE VIEW [svc].[uvw_PrivateKeys]
AS
SELECT        Id, UserId, PublicKeyId, KeyValue, KeyAlgo, KeyPass, KeyFormat, Enabled, Created, LastUpdated, Immutable
FROM            dbo.tbl_PrivateKeys
