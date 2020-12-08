
CREATE PROCEDURE [svc].[usp_Setting_Insert]
     @UserId				UNIQUEIDENTIFIER
    ,@ConfigKey				NVARCHAR (MAX) 
    ,@ConfigValue			NVARCHAR (MAX) 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

		DECLARE @SETTINGID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_Setting]
			(
			 Id         
			,UserId           
			,ConfigKey   
			,ConfigValue
			,IsDeletable
			,CreatedUtc
			)
		VALUES
			(
			 @SETTINGID          
			,@UserId         
			,@ConfigKey
			,@ConfigValue
			,@IsDeletable
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_Setting] WHERE Id = @SETTINGID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END