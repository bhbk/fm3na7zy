
CREATE VIEW [svc].[uvw_Folder]
AS
SELECT
	Id
	,UserId
	,ParentId
	,VirtualName
	,IsReadOnly
	,CreatedUtc
	,LastAccessedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_Folder]
