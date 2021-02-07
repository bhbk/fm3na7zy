
CREATE VIEW [svc].[uvw_NetworkActionType]

AS

SELECT Id
      ,Name
	  ,IsEnabled
	  ,IsEditable
	  ,IsDeletable
FROM [dbo].[tbl_NetworkActionType]