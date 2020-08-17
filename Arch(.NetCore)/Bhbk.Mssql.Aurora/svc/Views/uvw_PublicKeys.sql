
CREATE VIEW svc.uvw_PublicKeys
AS
SELECT        Id, IdentityId, PrivateKeyId, KeyValue, KeyAlgo, KeyFormat, SigValue, SigAlgo, Comment, Enabled, Created, LastUpdated, Immutable
FROM            dbo.tbl_PublicKeys
