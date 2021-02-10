
CREATE   VIEW [svc].[uvw_Alert]
AS
SELECT
	Id
	,UserId
	,OnDelete
	,OnDownload
	,OnUpload
	,ToDisplayName
	,ToEmailAddress
	,ToPhoneNumber
	,IsEnabled
	,CreatedUtc

FROM
	[dbo].[tbl_Alert]
