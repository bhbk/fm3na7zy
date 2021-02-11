CREATE VIEW [svc].[uvw_Network]
AS
SELECT Id,
	UserId,
	SequenceId,
	Address,
	ActionTypeId,
	IsEnabled,
	IsDeletable,
	Comment,
	CreatedUtc
FROM [dbo].[tbl_Network]
