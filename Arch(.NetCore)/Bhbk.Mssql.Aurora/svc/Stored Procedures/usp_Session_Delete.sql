
CREATE PROCEDURE [svc].[usp_Session_Delete]
	@Id UNIQUEIDENTIFIER

AS

SET NOCOUNT ON;

DELETE FROM [dbo].[tbl_Session]
WHERE Id = @Id;