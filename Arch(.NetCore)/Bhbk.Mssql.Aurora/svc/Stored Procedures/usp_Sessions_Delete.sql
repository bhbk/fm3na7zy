CREATE PROCEDURE [svc].[usp_Sessions_Delete] @UserId UNIQUEIDENTIFIER NULL,
	@DeleteBeforeDate [datetimeoffset] (7),
	@LocalEndPoint NVARCHAR(128) = NULL,
	@RemoteEndPoint NVARCHAR(128) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DELETE
	FROM [dbo].[tbl_Session]
	WHERE (
			UserId = @UserId
			OR @UserId IS NULL
			)
		AND (CreatedUtc <= @DeleteBeforeDate)
		AND (
			LocalEndPoint = @LocalEndPoint
			OR @LocalEndPoint IS NULL
			)
		AND (
			RemoteEndPoint = @RemoteEndPoint
			OR @RemoteEndPoint IS NULL
			);
END
