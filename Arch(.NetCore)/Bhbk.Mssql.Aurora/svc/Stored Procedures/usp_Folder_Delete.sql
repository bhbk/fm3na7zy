﻿CREATE PROCEDURE [svc].[usp_Folder_Delete] @Id UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
		SELECT *
		FROM [dbo].[tbl_Folder]
		WHERE Id = @Id

		DELETE [dbo].[tbl_Folder]
		WHERE Id = @Id
	END TRY

	BEGIN CATCH
		THROW;
	END CATCH
END
