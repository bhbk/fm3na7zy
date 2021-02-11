﻿CREATE PROCEDURE [svc].[usp_LoginAuthType_Insert] @Id INT,
	@Name NVARCHAR(16),
	@Description NVARCHAR(256),
	@IsEnabled BIT,
	@IsEditable BIT,
	@IsDeletable BIT
AS
SET NOCOUNT ON;

INSERT INTO [dbo].[tbl_LoginAuthType] (
	Id,
	Name,
	Description,
	IsEnabled,
	IsEditable,
	IsDeletable
	)
VALUES (
	@Id,
	@Name,
	@Description,
	@IsEnabled,
	@IsEditable,
	@IsDeletable
	);

/*  Select all entity values to return
        ----------------------------------------------------
       */
SELECT *
FROM [dbo].[tbl_LoginAuthType]
WHERE Id = @Id
