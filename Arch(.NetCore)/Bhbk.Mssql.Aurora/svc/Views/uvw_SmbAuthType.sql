
CREATE VIEW [svc].[uvw_SmbAuthType]

AS

SELECT Id
      ,Name
	  ,IsEnabled
	  ,IsEditable
	  ,IsDeletable
FROM [dbo].[tbl_SmbAuthType]