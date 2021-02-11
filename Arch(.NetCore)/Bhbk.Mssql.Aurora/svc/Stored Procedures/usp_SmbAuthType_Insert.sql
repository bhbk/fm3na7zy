CREATE PROCEDURE [svc].[usp_SmbAuthType_Insert] @Id INT,
	@Name NVARCHAR(32),
	@Description NVARCHAR(256),
	@IsEnabled BIT,
	@IsEditable BIT,
	@IsDeletable BIT
AS
SET NOCOUNT ON;

INSERT INTO [dbo].[tbl_SmbAuthType] (
	Id,
	Name,
	IsEnabled,
	Description,
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
FROM [dbo].[tbl_SmbAuthType]
WHERE Id = @Id
