
CREATE PROCEDURE [svc].[usp_Session_Delete]
	@Id uniqueidentifier

AS

SET NOCOUNT ON;

DELETE FROM [dbo].[tbl_Session]
WHERE Id = @Id;
