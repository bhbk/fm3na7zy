
CREATE VIEW [svc].[uvw_PublicKeys]
AS
SELECT        Id, IdentityId, PrivateKeyId, KeyValue, KeyAlgo, KeyFormat, SigValue, SigAlgo, Hostname, Enabled, Created, LastUpdated, Immutable
FROM            dbo.tbl_PublicKeys
