
CREATE PROCEDURE [svc].[usp_Session_Update]
    @Id						    UNIQUEIDENTIFIER
   ,@IdentityId					UNIQUEIDENTIFIER
   ,@CallPath					VARCHAR(256)
   ,@Details					VARCHAR(MAX)
   ,@LocalEndPoint				VARCHAR(128)
   ,@LocalSoftwareIdentifier	VARCHAR(128)
   ,@RemoteEndPoint				VARCHAR(128)
   ,@RemoteSoftwareIdentifier	VARCHAR(128)
   ,@IsActive					BIT

AS

SET NOCOUNT ON;

DECLARE @UPDATED DATETIMEOFFSET = GETUTCDATE()

UPDATE [dbo].[tbl_Session] 
SET  IdentityId					=	@IdentityId
    ,CallPath					=   @CallPath
    ,Details					=   @Details
    ,LocalEndPoint				=   @LocalEndPoint
    ,LocalSoftwareIdentifier    =   @LocalSoftwareIdentifier
    ,RemoteEndPoint				=   @RemoteEndPoint
    ,RemoteSoftwareIdentifier	=   @RemoteSoftwareIdentifier
    ,IsActive                   =   @IsActive
WHERE Id = @Id;

    /*  Select all entity values to return
        ----------------------------------------------------
       */
	SELECT * FROM [dbo].[tbl_Session] WHERE Id = @Id