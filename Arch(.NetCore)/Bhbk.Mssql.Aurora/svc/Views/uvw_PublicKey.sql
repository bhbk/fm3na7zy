
CREATE VIEW svc.uvw_PublicKey
AS
SELECT        Id, IdentityId, PrivateKeyId, KeyValue, KeyAlgo, KeyFormat, SigValue, SigAlgo, Comment, Enabled, Deletable, Created, LastUpdated
FROM            dbo.tbl_PublicKey
