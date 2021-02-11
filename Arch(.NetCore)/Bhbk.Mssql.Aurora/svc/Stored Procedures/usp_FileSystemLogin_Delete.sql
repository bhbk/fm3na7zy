CREATE PROCEDURE [svc].[usp_FileSystemLogin_Delete] @FileSystemId UNIQUEIDENTIFIER,
	@UserId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

DELETE [dbo].[tbl_FileSystemLogin]
WHERE FileSystemId = @FileSystemId
	AND UserId = @UserId;
