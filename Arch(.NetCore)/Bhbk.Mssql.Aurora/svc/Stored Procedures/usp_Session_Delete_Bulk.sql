
CREATE PROCEDURE [svc].[usp_Session_Delete_Bulk]
	@UserId uniqueidentifier NULL,
	@DeleteBeforeDate [datetimeoffset](7),
	@LocalEndPoint nvarchar(128) = NULL,
	@RemoteEndPoint nvarchar(128) = NULL


AS

BEGIN
SET NOCOUNT ON;

DELETE FROM [dbo].[tbl_Session]
WHERE (UserId = @UserId OR @UserId IS NULL)
	AND (CreatedUtc <= @DeleteBeforeDate)
	AND (LocalEndPoint = @LocalEndPoint OR @LocalEndPoint IS NULL)
	AND (RemoteEndPoint = @RemoteEndPoint OR @RemoteEndPoint IS NULL)
;

END