﻿CREATE PROCEDURE [svc].[usp_PublicKeyFormatType_Update] @Id INT,
	@Name NVARCHAR(16),
	@Description NVARCHAR(256),
	@IsEnabled BIT,
	@IsEditable BIT,
	@IsDeletable BIT
AS
SET NOCOUNT ON;

DECLARE @Error_Message VARCHAR(MAX);

IF EXISTS (
		SELECT 1
		FROM [dbo].[tbl_PublicKeyFormatType]
		WHERE Id = @Id
			AND IsEditable = 0
		)
BEGIN
	SET @Error_Message = FORMATMESSAGE('EDIT not allowed, Id (%s) is not editable.', CONVERT(VARCHAR, COALESCE(@Id, '')));

	THROW 50000,
		@Error_Message,
		1;
END;

UPDATE [dbo].[tbl_PublicKeyFormatType]
SET Name = @Name,
	Description = @Description,
	IsEnabled = @IsEnabled,
	IsEditable = @IsEditable,
	IsDeletable = @IsDeletable
WHERE Id = @Id;

/*  Select all entity values to return
        ----------------------------------------------------
       */
SELECT *
FROM [dbo].[tbl_PublicKeyFormatType]
WHERE Id = @Id
