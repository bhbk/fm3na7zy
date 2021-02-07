
CREATE PROCEDURE [svc].[usp_FileSystemLogin_Delete]
   @FileSystemId uniqueidentifier
  ,@UserId uniqueidentifier

AS

SET NOCOUNT ON;

DELETE [dbo].[tbl_FileSystemLogin] 
WHERE FileSystemId = @FileSystemId
	AND UserId = @UserId;