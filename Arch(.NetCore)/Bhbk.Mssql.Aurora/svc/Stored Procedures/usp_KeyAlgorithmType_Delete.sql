CREATE PROCEDURE [svc].[usp_KeyAlgorithmType_Delete] @Id INT
AS
SET NOCOUNT ON;

DECLARE @Error_Message VARCHAR(MAX);

IF EXISTS (
		SELECT 1
		FROM [dbo].[tbl_KeyAlgorithmType]
		WHERE Id = @Id
			AND IsDeletable = 0
		)
BEGIN
	SET @Error_Message = FORMATMESSAGE('DELETE not allowed, Id (%s) is not deletable.', CONVERT(VARCHAR, COALESCE(@Id, '')));

	THROW 50000,
		@Error_Message,
		1;
END;

DELETE
FROM [dbo].[tbl_KeyAlgorithmType]
WHERE Id = @Id;
