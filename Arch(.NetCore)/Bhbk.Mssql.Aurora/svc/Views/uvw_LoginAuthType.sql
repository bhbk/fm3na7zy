CREATE VIEW [svc].[uvw_LoginAuthType]
AS
SELECT Id,
	Name,
	IsEnabled,
	IsEditable,
	IsDeletable
FROM [dbo].[tbl_LoginAuthType]
