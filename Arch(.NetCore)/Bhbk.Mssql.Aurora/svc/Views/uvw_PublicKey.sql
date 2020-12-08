
CREATE VIEW [svc].[uvw_PublicKey]
AS
SELECT
	Id
	,UserId
	,PrivateKeyId
	,KeyValue
	,KeyAlgo
	,KeyFormat
	,SigValue
	,SigAlgo
	,Comment
	,IsEnabled
	,IsDeletable
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_PublicKey]
