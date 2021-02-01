
CREATE PROCEDURE [svc].[usp_Session_Update]
    @Id						    UNIQUEIDENTIFIER
   ,@CallPath					VARCHAR(256)
   ,@Details					VARCHAR(MAX)
   ,@LocalEndPoint				VARCHAR(128)
   ,@LocalSoftwareIdentifier	VARCHAR(128)
   ,@RemoteEndPoint				VARCHAR(128)
   ,@RemoteSoftwareIdentifier	VARCHAR(128)
   ,@IsActive					BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

    	BEGIN TRANSACTION;

        DECLARE @UPDATED DATETIMEOFFSET = GETUTCDATE()

        UPDATE [dbo].[tbl_Session] 
        SET  
            CallPath					=   @CallPath
            ,Details					=   @Details
            ,LocalEndPoint				=   @LocalEndPoint
            ,LocalSoftwareIdentifier    =   @LocalSoftwareIdentifier
            ,RemoteEndPoint				=   @RemoteEndPoint
            ,RemoteSoftwareIdentifier	=   @RemoteSoftwareIdentifier
            ,IsActive                   =   @IsActive
        WHERE Id = @Id;

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

	    SELECT * FROM [dbo].[tbl_Session] 
            WHERE Id = @Id

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END
