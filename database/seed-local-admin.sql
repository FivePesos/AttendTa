/*
   Seeds the one-time local MVP test admin in AttendTaMVP_Test, AttendTa_IT_<guid>,
   or AttendTa_Demo.
   Supply AdminPasswordHash from the Auth password hasher; never pass plaintext.
   This script does not define or distribute a password or hash.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'AttendTaMVP_Test'
   AND DB_NAME() <> N'AttendTa_Demo'
   AND (DB_NAME() NOT LIKE N'AttendTa_IT[_]%' OR LEN(DB_NAME()) <= LEN(N'AttendTa_IT_'))
    THROW 51015, 'This admin seed may only run in AttendTaMVP_Test, AttendTa_IT_<guid>, or AttendTa_Demo.', 1;
GO

DECLARE @AdminPasswordHash varchar(255) = '$(AdminPasswordHash)';
DECLARE @ExistingUserId int;
DECLARE @ExistingRoleId tinyint;
DECLARE @ExistingIsActive bit;

SELECT
    @ExistingUserId = [user_id],
    @ExistingRoleId = [role_id],
    @ExistingIsActive = [is_active]
FROM dbo.users
WHERE [email] = N'admin@gmail.com';

IF @ExistingUserId IS NOT NULL
BEGIN
    IF @ExistingRoleId <> 1 OR @ExistingIsActive <> 1
        THROW 51016, 'admin@gmail.com exists but is not an active admin; resolve manually.', 1;
END
ELSE
BEGIN
    IF NULLIF(LTRIM(RTRIM(@AdminPasswordHash)), '') IS NULL
       OR @AdminPasswordHash = '$(AdminPasswordHash)'
        THROW 51017, 'Supply an Auth-generated AdminPasswordHash; plaintext passwords are not accepted.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.users ([full_name], [email], [password_hash], [role_id], [is_active], [created_by])
        VALUES (N'AttendTa Local Admin', N'admin@gmail.com', @AdminPasswordHash, 1, 1, NULL);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END
GO

SELECT [user_id], [full_name], [email], [role_id], [is_active], [created_by]
FROM dbo.users
WHERE [email] = N'admin@gmail.com';
GO
