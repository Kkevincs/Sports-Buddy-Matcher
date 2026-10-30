IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028142953_InitialCreate'
)
BEGIN
    CREATE TABLE [Location] (
        [Id] int NOT NULL IDENTITY,
        [Country] nvarchar(100) NOT NULL,
        [City] nvarchar(100) NOT NULL,
        [Street] nvarchar(255) NOT NULL,
        CONSTRAINT [PK_Location] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028142953_InitialCreate'
)
BEGIN
    CREATE TABLE [Skill] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Skill] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028142953_InitialCreate'
)
BEGIN
    CREATE TABLE [Event] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Date] datetime2 NOT NULL,
        [Location] int NOT NULL,
        CONSTRAINT [PK_Event] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Event_Location_Location] FOREIGN KEY ([Location]) REFERENCES [Location] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028142953_InitialCreate'
)
BEGIN
    CREATE TABLE [User] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(255) NOT NULL,
        [Email] nvarchar(255) NOT NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [Location] int NOT NULL,
        CONSTRAINT [PK_User] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_User_Location_Location] FOREIGN KEY ([Location]) REFERENCES [Location] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028142953_InitialCreate'
)
BEGIN
    CREATE TABLE [PlayerSkill] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [SkillId] int NOT NULL,
        [SkillLevel] int NOT NULL,
        CONSTRAINT [PK_PlayerSkill] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PlayerSkill_Skill_SkillId] FOREIGN KEY ([SkillId]) REFERENCES [Skill] ([Id]),
        CONSTRAINT [FK_PlayerSkill_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028142953_InitialCreate'
)
BEGIN
    CREATE TABLE [UserEvent] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [EventId] int NOT NULL,
        CONSTRAINT [PK_UserEvent] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserEvent_Event_EventId] FOREIGN KEY ([EventId]) REFERENCES [Event] ([Id]),
        CONSTRAINT [FK_UserEvent_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028142953_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Event_Location] ON [Event] ([Location]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028142953_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PlayerSkill_SkillId] ON [PlayerSkill] ([SkillId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028142953_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PlayerSkill_UserId_SkillId] ON [PlayerSkill] ([UserId], [SkillId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028142953_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Skill_Name] ON [Skill] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028142953_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_User_Email] ON [User] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028142953_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_User_Location] ON [User] ([Location]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028142953_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserEvent_EventId] ON [UserEvent] ([EventId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028142953_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserEvent_UserId_EventId] ON [UserEvent] ([UserId], [EventId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028142953_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028142953_InitialCreate', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150246_AddLocationCoordinates'
)
BEGIN
    ALTER TABLE [Location] ADD [Latitude] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150246_AddLocationCoordinates'
)
BEGIN
    ALTER TABLE [Location] ADD [Longitude] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028150246_AddLocationCoordinates'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028150246_AddLocationCoordinates', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028151518_AddLocationPostcode'
)
BEGIN
    ALTER TABLE [Location] ADD [Postcode] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028151518_AddLocationPostcode'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028151518_AddLocationPostcode', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028151900_RequireLocationPostcode'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Location]') AND [c].[name] = N'Postcode');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Location] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Location] ALTER COLUMN [Postcode] nvarchar(max) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028151900_RequireLocationPostcode'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028151900_RequireLocationPostcode', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028154804_AddUserPreferredRange'
)
BEGIN
    ALTER TABLE [User] ADD [PreferredRange] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028154804_AddUserPreferredRange'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028154804_AddUserPreferredRange', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028155417_AddPlayerAvailability'
)
BEGIN
    CREATE TABLE [Availability] (
        [Id] int NOT NULL IDENTITY,
        [PlayerSkillId] int NOT NULL,
        [DayOfWeek] int NOT NULL,
        [StartTime] time NOT NULL,
        [EndTime] time NOT NULL,
        CONSTRAINT [PK_Availability] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Availability_PlayerSkill_PlayerSkillId] FOREIGN KEY ([PlayerSkillId]) REFERENCES [PlayerSkill] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028155417_AddPlayerAvailability'
)
BEGIN
    CREATE INDEX [IX_Availability_PlayerSkillId] ON [Availability] ([PlayerSkillId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028155417_AddPlayerAvailability'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028155417_AddPlayerAvailability', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028165030_MoveAvailabilityToUser'
)
BEGIN
    ALTER TABLE [Availability] DROP CONSTRAINT [FK_Availability_PlayerSkill_PlayerSkillId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028165030_MoveAvailabilityToUser'
)
BEGIN
    EXEC sp_rename N'[Availability].[PlayerSkillId]', N'UserId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028165030_MoveAvailabilityToUser'
)
BEGIN

                    UPDATE a
                    SET a.UserId = p.UserId
                    FROM Availability a
                    INNER JOIN PlayerSkill p ON a.UserId = p.Id;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028165030_MoveAvailabilityToUser'
)
BEGIN
    EXEC sp_rename N'[Availability].[IX_Availability_PlayerSkillId]', N'IX_Availability_UserId', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028165030_MoveAvailabilityToUser'
)
BEGIN
    ALTER TABLE [Availability] ADD CONSTRAINT [FK_Availability_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028165030_MoveAvailabilityToUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028165030_MoveAvailabilityToUser', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028174106_AddEventMatchingDetails'
)
BEGIN
    ALTER TABLE [Event] ADD [EndDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028174106_AddEventMatchingDetails'
)
BEGIN
    ALTER TABLE [Event] ADD [SkillId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028174106_AddEventMatchingDetails'
)
BEGIN
    ALTER TABLE [Event] ADD [SkillLevel] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028174106_AddEventMatchingDetails'
)
BEGIN
    CREATE INDEX [IX_Event_SkillId] ON [Event] ([SkillId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028174106_AddEventMatchingDetails'
)
BEGIN
    ALTER TABLE [Event] ADD CONSTRAINT [FK_Event_Skill_SkillId] FOREIGN KEY ([SkillId]) REFERENCES [Skill] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028174106_AddEventMatchingDetails'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028174106_AddEventMatchingDetails', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028184553_AddEventCapacity'
)
BEGIN
    ALTER TABLE [Event] ADD [MaxParticipants] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028184553_AddEventCapacity'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028184553_AddEventCapacity', N'8.0.22');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028192036_AddNoticeboardProfilesAndConnections'
)
BEGIN
    EXEC sp_rename N'[User].[PreferredRange]', N'PreferredRangeKm', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028192036_AddNoticeboardProfilesAndConnections'
)
BEGIN
    ALTER TABLE [User] ADD [Bio] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028192036_AddNoticeboardProfilesAndConnections'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[User]') AND [c].[name] = N'PreferredRangeKm');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [User] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [User] ALTER COLUMN [PreferredRangeKm] decimal(12,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028192036_AddNoticeboardProfilesAndConnections'
)
BEGIN
    ALTER TABLE [User] ADD [ProfilePictureUrl] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028192036_AddNoticeboardProfilesAndConnections'
)
BEGIN
    ALTER TABLE [Event] ADD [Bio] nvarchar(4000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028192036_AddNoticeboardProfilesAndConnections'
)
BEGIN
    ALTER TABLE [Event] ADD [CreatedById] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028192036_AddNoticeboardProfilesAndConnections'
)
BEGIN
    ALTER TABLE [Event] ADD [PictureUrl] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028192036_AddNoticeboardProfilesAndConnections'
)
BEGIN
    CREATE TABLE [Connection] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [OtherUserId] int NOT NULL,
        [RequestedById] int NOT NULL,
        [IsAccepted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Connection] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Connection_Order] CHECK ([UserId] < [OtherUserId]),
        CONSTRAINT [FK_Connection_User_OtherUserId] FOREIGN KEY ([OtherUserId]) REFERENCES [User] ([Id]),
        CONSTRAINT [FK_Connection_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028192036_AddNoticeboardProfilesAndConnections'
)
BEGIN
    CREATE INDEX [IX_Event_CreatedById] ON [Event] ([CreatedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028192036_AddNoticeboardProfilesAndConnections'
)
BEGIN
    CREATE INDEX [IX_Connection_OtherUserId] ON [Connection] ([OtherUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028192036_AddNoticeboardProfilesAndConnections'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Connection_UserId_OtherUserId] ON [Connection] ([UserId], [OtherUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028192036_AddNoticeboardProfilesAndConnections'
)
BEGIN
    ALTER TABLE [Event] ADD CONSTRAINT [FK_Event_User_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [User] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261028192036_AddNoticeboardProfilesAndConnections'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261028192036_AddNoticeboardProfilesAndConnections', N'8.0.22');
END;
GO

COMMIT;
GO
