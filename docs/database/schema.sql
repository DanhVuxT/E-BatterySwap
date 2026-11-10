/*
    E-BatterySwap - SQL Server schema
    Source: docs/database/erd-design.md
    Scope: 14 core entities for accounts, vehicles, batteries, stations,
           swapping transactions, simulator commands/events and alerts.

    Implementation notes:
    - BIGINT surrogate primary keys use IDENTITY(1,1).
    - Timestamps are supplied by the application; UTC is recommended.
    - Foreign keys use SQL Server's default NO ACTION behavior to preserve history.
    - Cross-row/business rules that cannot be safely expressed as CHECK constraints
      are documented next to the relevant table and must be enforced transactionally
      by the Backend API.
    - This script creates a new schema and is intended to run once in an empty database.
*/

SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    /* =========================================================
       GROUP 1 - ACCOUNTS AND VEHICLES
       ========================================================= */

    CREATE TABLE dbo.Role
    (
        RoleId          BIGINT IDENTITY(1,1) NOT NULL,
        RoleCode        VARCHAR(30) NOT NULL,
        RoleName        NVARCHAR(100) NOT NULL,
        Description     NVARCHAR(255) NULL,
        IsActive        BIT NOT NULL,

        CONSTRAINT PK_Role PRIMARY KEY CLUSTERED (RoleId),
        CONSTRAINT UQ_Role_RoleCode UNIQUE (RoleCode)
    );

    CREATE TABLE dbo.Users
    (
        UserId          BIGINT IDENTITY(1,1) NOT NULL,
        RoleId          BIGINT NOT NULL,
        FullName        NVARCHAR(150) NOT NULL,
        PhoneNumber     VARCHAR(20) NULL,
        Email           NVARCHAR(254) NOT NULL,
        PasswordHash    NVARCHAR(512) NOT NULL,
        Status          VARCHAR(20) NOT NULL,
        CreatedAt       DATETIME2(3) NOT NULL,
        UpdatedAt       DATETIME2(3) NULL,

        CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (UserId),
        CONSTRAINT UQ_Users_Email UNIQUE (Email),
        CONSTRAINT FK_Users_Role
            FOREIGN KEY (RoleId) REFERENCES dbo.Role(RoleId),
        CONSTRAINT CK_Users_Status
            CHECK (Status IN ('PENDING', 'ACTIVE', 'LOCKED', 'DISABLED'))
    );

    CREATE TABLE dbo.Vehicle
    (
        VehicleId               BIGINT IDENTITY(1,1) NOT NULL,
        UserId                  BIGINT NOT NULL,
        LicensePlate            NVARCHAR(20) NOT NULL,
        ModelName               NVARCHAR(100) NOT NULL,
        BatteryCapacityCount    TINYINT NOT NULL,
        Status                  VARCHAR(20) NOT NULL,
        CreatedAt               DATETIME2(3) NOT NULL,
        UpdatedAt               DATETIME2(3) NULL,

        CONSTRAINT PK_Vehicle PRIMARY KEY CLUSTERED (VehicleId),
        CONSTRAINT UQ_Vehicle_LicensePlate UNIQUE (LicensePlate),
        CONSTRAINT FK_Vehicle_Users
            FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT CK_Vehicle_BatteryCapacityCount
            CHECK (BatteryCapacityCount IN (1, 2)),
        CONSTRAINT CK_Vehicle_Status
            CHECK (Status IN ('ACTIVE', 'INACTIVE'))
    );

    /* =========================================================
       GROUP 2 - BATTERIES AND STATIONS

       Battery.CurrentSlotId references Slot, while Slot references
       Station. The Battery -> Slot FK is added after Slot exists.
       ========================================================= */

    CREATE TABLE dbo.Battery
    (
        BatteryId           BIGINT IDENTITY(1,1) NOT NULL,
        SerialNumber        NVARCHAR(100) NOT NULL,
        SOC                 DECIMAL(5,2) NULL,
        SOH                 DECIMAL(5,2) NULL,
        Temperature         DECIMAL(5,2) NULL,
        CycleCount          INT NOT NULL,
        OperationalStatus   VARCHAR(20) NOT NULL,
        LocationType        VARCHAR(20) NOT NULL,
        CurrentSlotId       BIGINT NULL,
        LastTelemetryAt     DATETIME2(3) NULL,
        CreatedAt           DATETIME2(3) NOT NULL,

        CONSTRAINT PK_Battery PRIMARY KEY CLUSTERED (BatteryId),
        CONSTRAINT UQ_Battery_SerialNumber UNIQUE (SerialNumber),
        CONSTRAINT CK_Battery_SOC CHECK (SOC BETWEEN 0 AND 100),
        CONSTRAINT CK_Battery_SOH CHECK (SOH BETWEEN 0 AND 100),
        CONSTRAINT CK_Battery_CycleCount CHECK (CycleCount >= 0),
        CONSTRAINT CK_Battery_OperationalStatus
            CHECK (OperationalStatus IN ('NORMAL', 'WARNING', 'FAULT', 'MAINTENANCE', 'RETIRED')),
        CONSTRAINT CK_Battery_LocationType
            CHECK (LocationType IN ('SLOT', 'VEHICLE', 'STORAGE', 'MAINTENANCE', 'IN_TRANSIT', 'UNKNOWN')),
        CONSTRAINT CK_Battery_LocationSlot
            CHECK
            (
                (LocationType = 'SLOT' AND CurrentSlotId IS NOT NULL)
                OR
                (LocationType <> 'SLOT' AND CurrentSlotId IS NULL)
            )
    );

    CREATE TABLE dbo.Station
    (
        StationId           BIGINT IDENTITY(1,1) NOT NULL,
        StationCode         VARCHAR(30) NOT NULL,
        StationName         NVARCHAR(150) NOT NULL,
        Address             NVARCHAR(255) NOT NULL,
        Latitude            DECIMAL(9,6) NULL,
        Longitude           DECIMAL(9,6) NULL,
        QRIdentifier        VARCHAR(100) NOT NULL,
        OperationalStatus   VARCHAR(20) NOT NULL,
        ConnectionStatus    VARCHAR(20) NOT NULL,
        LastSeenAt          DATETIME2(3) NULL,
        CreatedAt           DATETIME2(3) NOT NULL,

        CONSTRAINT PK_Station PRIMARY KEY CLUSTERED (StationId),
        CONSTRAINT UQ_Station_StationCode UNIQUE (StationCode),
        CONSTRAINT UQ_Station_QRIdentifier UNIQUE (QRIdentifier),
        CONSTRAINT CK_Station_Latitude CHECK (Latitude BETWEEN -90 AND 90),
        CONSTRAINT CK_Station_Longitude CHECK (Longitude BETWEEN -180 AND 180),
        CONSTRAINT CK_Station_OperationalStatus
            CHECK (OperationalStatus IN ('ACTIVE', 'INACTIVE', 'MAINTENANCE')),
        CONSTRAINT CK_Station_ConnectionStatus
            CHECK (ConnectionStatus IN ('ONLINE', 'OFFLINE'))
    );

    CREATE TABLE dbo.Slot
    (
        SlotId              BIGINT IDENTITY(1,1) NOT NULL,
        StationId           BIGINT NOT NULL,
        SlotNumber          SMALLINT NOT NULL,
        DoorStatus          VARCHAR(20) NOT NULL,
        LockStatus          VARCHAR(20) NOT NULL,
        OperationalStatus   VARCHAR(20) NOT NULL,
        CreatedAt           DATETIME2(3) NOT NULL,

        CONSTRAINT PK_Slot PRIMARY KEY CLUSTERED (SlotId),
        CONSTRAINT UQ_Slot_Station_SlotNumber UNIQUE (StationId, SlotNumber),
        CONSTRAINT FK_Slot_Station
            FOREIGN KEY (StationId) REFERENCES dbo.Station(StationId),
        CONSTRAINT CK_Slot_DoorStatus
            CHECK (DoorStatus IN ('OPEN', 'CLOSED', 'UNKNOWN')),
        CONSTRAINT CK_Slot_LockStatus
            CHECK (LockStatus IN ('LOCKED', 'UNLOCKED', 'UNKNOWN')),
        CONSTRAINT CK_Slot_OperationalStatus
            CHECK (OperationalStatus IN ('AVAILABLE', 'DISABLED', 'FAULT', 'MAINTENANCE'))
    );

    ALTER TABLE dbo.Battery
        ADD CONSTRAINT FK_Battery_CurrentSlot
        FOREIGN KEY (CurrentSlotId) REFERENCES dbo.Slot(SlotId);

    CREATE UNIQUE NONCLUSTERED INDEX UX_Battery_CurrentSlotId_NotNull
        ON dbo.Battery(CurrentSlotId)
        WHERE CurrentSlotId IS NOT NULL;

    CREATE TABLE dbo.BatteryAssignment
    (
        AssignmentId        BIGINT IDENTITY(1,1) NOT NULL,
        BatteryId           BIGINT NOT NULL,
        VehicleId           BIGINT NOT NULL,
        AssignedAt          DATETIME2(3) NOT NULL,
        EndedAt             DATETIME2(3) NULL,
        AssignmentSource    VARCHAR(30) NOT NULL,
        RecordedByUserId    BIGINT NULL,

        CONSTRAINT PK_BatteryAssignment PRIMARY KEY CLUSTERED (AssignmentId),
        CONSTRAINT FK_BatteryAssignment_Battery
            FOREIGN KEY (BatteryId) REFERENCES dbo.Battery(BatteryId),
        CONSTRAINT FK_BatteryAssignment_Vehicle
            FOREIGN KEY (VehicleId) REFERENCES dbo.Vehicle(VehicleId),
        CONSTRAINT FK_BatteryAssignment_RecordedByUser
            FOREIGN KEY (RecordedByUserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT CK_BatteryAssignment_AssignmentSource
            CHECK (AssignmentSource IN ('INITIAL_HANDOVER', 'SWAP', 'MANUAL_ADJUSTMENT')),
        CONSTRAINT CK_BatteryAssignment_Time
            CHECK (EndedAt IS NULL OR EndedAt >= AssignedAt)
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_BatteryAssignment_ActiveBattery
        ON dbo.BatteryAssignment(BatteryId)
        WHERE EndedAt IS NULL;

    CREATE TABLE dbo.BatteryTelemetry
    (
        TelemetryId     BIGINT IDENTITY(1,1) NOT NULL,
        BatteryId       BIGINT NOT NULL,
        SOC             DECIMAL(5,2) NOT NULL,
        SOH             DECIMAL(5,2) NULL,
        Temperature     DECIMAL(5,2) NULL,
        MeasuredAt      DATETIME2(3) NOT NULL,
        ReceivedAt      DATETIME2(3) NOT NULL,
        Source          VARCHAR(20) NOT NULL,

        CONSTRAINT PK_BatteryTelemetry PRIMARY KEY CLUSTERED (TelemetryId),
        CONSTRAINT FK_BatteryTelemetry_Battery
            FOREIGN KEY (BatteryId) REFERENCES dbo.Battery(BatteryId),
        CONSTRAINT CK_BatteryTelemetry_SOC CHECK (SOC BETWEEN 0 AND 100),
        CONSTRAINT CK_BatteryTelemetry_SOH CHECK (SOH BETWEEN 0 AND 100),
        CONSTRAINT CK_BatteryTelemetry_Source
            CHECK (Source IN ('SIMULATOR', 'STATION', 'MANUAL'))
    );

    CREATE NONCLUSTERED INDEX IX_BatteryTelemetry_Battery_MeasuredAt
        ON dbo.BatteryTelemetry(BatteryId, MeasuredAt DESC);

    CREATE TABLE dbo.ChargingSession
    (
        ChargingSessionId  BIGINT IDENTITY(1,1) NOT NULL,
        BatteryId          BIGINT NOT NULL,
        SlotId             BIGINT NOT NULL,
        StartedAt          DATETIME2(3) NOT NULL,
        EndedAt            DATETIME2(3) NULL,
        StartSOC           DECIMAL(5,2) NULL,
        EndSOC             DECIMAL(5,2) NULL,
        Status             VARCHAR(20) NOT NULL,
        StopReason         NVARCHAR(255) NULL,

        CONSTRAINT PK_ChargingSession PRIMARY KEY CLUSTERED (ChargingSessionId),
        CONSTRAINT FK_ChargingSession_Battery
            FOREIGN KEY (BatteryId) REFERENCES dbo.Battery(BatteryId),
        CONSTRAINT FK_ChargingSession_Slot
            FOREIGN KEY (SlotId) REFERENCES dbo.Slot(SlotId),
        CONSTRAINT CK_ChargingSession_StartSOC CHECK (StartSOC BETWEEN 0 AND 100),
        CONSTRAINT CK_ChargingSession_EndSOC CHECK (EndSOC BETWEEN 0 AND 100),
        CONSTRAINT CK_ChargingSession_Status
            CHECK (Status IN ('CHARGING', 'COMPLETED', 'INTERRUPTED', 'FAILED')),
        CONSTRAINT CK_ChargingSession_Status_EndTime
            CHECK
            (
                (Status = 'CHARGING' AND EndedAt IS NULL)
                OR
                (Status IN ('COMPLETED', 'INTERRUPTED', 'FAILED') AND EndedAt IS NOT NULL)
            ),
        CONSTRAINT CK_ChargingSession_Time
            CHECK (EndedAt IS NULL OR EndedAt >= StartedAt)
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_ChargingSession_ActiveBattery
        ON dbo.ChargingSession(BatteryId)
        WHERE Status = 'CHARGING';

    CREATE UNIQUE NONCLUSTERED INDEX UX_ChargingSession_ActiveSlot
        ON dbo.ChargingSession(SlotId)
        WHERE Status = 'CHARGING';

    /* =========================================================
       GROUP 3 - BATTERY SWAP TRANSACTIONS
       ========================================================= */

    CREATE TABLE dbo.SwapSession
    (
        SwapSessionId          BIGINT IDENTITY(1,1) NOT NULL,
        VehicleId              BIGINT NOT NULL,
        StationId              BIGINT NOT NULL,
        RequestedBatteryCount  TINYINT NOT NULL,
        Status                 VARCHAR(30) NOT NULL,
        StartedAt              DATETIME2(3) NOT NULL,
        EndedAt                DATETIME2(3) NULL,
        FailureReason           NVARCHAR(255) NULL,

        CONSTRAINT PK_SwapSession PRIMARY KEY CLUSTERED (SwapSessionId),
        CONSTRAINT FK_SwapSession_Vehicle
            FOREIGN KEY (VehicleId) REFERENCES dbo.Vehicle(VehicleId),
        CONSTRAINT FK_SwapSession_Station
            FOREIGN KEY (StationId) REFERENCES dbo.Station(StationId),
        CONSTRAINT CK_SwapSession_RequestedBatteryCount
            CHECK (RequestedBatteryCount IN (1, 2)),
        CONSTRAINT CK_SwapSession_Status
            CHECK (Status IN ('PENDING', 'IN_PROGRESS', 'COMPLETED', 'PARTIALLY_COMPLETED', 'FAILED', 'CANCELLED')),
        CONSTRAINT CK_SwapSession_Status_EndTime
            CHECK
            (
                (Status IN ('PENDING', 'IN_PROGRESS') AND EndedAt IS NULL)
                OR
                (Status IN ('COMPLETED', 'PARTIALLY_COMPLETED', 'FAILED', 'CANCELLED') AND EndedAt IS NOT NULL)
            ),
        CONSTRAINT CK_SwapSession_Time
            CHECK (EndedAt IS NULL OR EndedAt >= StartedAt)
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_SwapSession_OneOpenPerVehicle
        ON dbo.SwapSession(VehicleId)
        WHERE EndedAt IS NULL;

    CREATE TABLE dbo.SwapItem
    (
        SwapItemId          BIGINT IDENTITY(1,1) NOT NULL,
        SwapSessionId       BIGINT NOT NULL,
        SequenceNumber      TINYINT NOT NULL,
        ReturnedBatteryId   BIGINT NULL,
        IssuedBatteryId     BIGINT NULL,
        ReturnSlotId        BIGINT NULL,
        IssueSlotId         BIGINT NULL,
        Status              VARCHAR(30) NOT NULL,
        ReturnedAt          DATETIME2(3) NULL,
        IssuedAt            DATETIME2(3) NULL,
        FailureReason       NVARCHAR(255) NULL,
        CreatedAt           DATETIME2(3) NOT NULL,

        CONSTRAINT PK_SwapItem PRIMARY KEY CLUSTERED (SwapItemId),
        CONSTRAINT UQ_SwapItem_Session_Sequence UNIQUE (SwapSessionId, SequenceNumber),
        CONSTRAINT FK_SwapItem_SwapSession
            FOREIGN KEY (SwapSessionId) REFERENCES dbo.SwapSession(SwapSessionId),
        CONSTRAINT FK_SwapItem_ReturnedBattery
            FOREIGN KEY (ReturnedBatteryId) REFERENCES dbo.Battery(BatteryId),
        CONSTRAINT FK_SwapItem_IssuedBattery
            FOREIGN KEY (IssuedBatteryId) REFERENCES dbo.Battery(BatteryId),
        CONSTRAINT FK_SwapItem_ReturnSlot
            FOREIGN KEY (ReturnSlotId) REFERENCES dbo.Slot(SlotId),
        CONSTRAINT FK_SwapItem_IssueSlot
            FOREIGN KEY (IssueSlotId) REFERENCES dbo.Slot(SlotId),
        CONSTRAINT CK_SwapItem_SequenceNumber
            CHECK (SequenceNumber IN (1, 2)),
        CONSTRAINT CK_SwapItem_Status
            CHECK (Status IN ('PENDING', 'RETURN_CONFIRMED', 'ISSUE_CONFIRMED', 'COMPLETED', 'FAILED', 'CANCELLED')),
        CONSTRAINT CK_SwapItem_DifferentBatteries
            CHECK
            (
                ReturnedBatteryId IS NULL
                OR IssuedBatteryId IS NULL
                OR ReturnedBatteryId <> IssuedBatteryId
            ),
        CONSTRAINT CK_SwapItem_ReturnTriple
            CHECK
            (
                (ReturnedBatteryId IS NULL AND ReturnSlotId IS NULL AND ReturnedAt IS NULL)
                OR
                (ReturnedBatteryId IS NOT NULL AND ReturnSlotId IS NOT NULL AND ReturnedAt IS NOT NULL)
            ),
        CONSTRAINT CK_SwapItem_IssueTriple
            CHECK
            (
                (IssuedBatteryId IS NULL AND IssueSlotId IS NULL AND IssuedAt IS NULL)
                OR
                (IssuedBatteryId IS NOT NULL AND IssueSlotId IS NOT NULL AND IssuedAt IS NOT NULL)
            )
    );

    /* =========================================================
       GROUP 4 - SIMULATOR COMMANDS, EVENTS AND ALERTS
       ========================================================= */

    CREATE TABLE dbo.StationCommand
    (
        CommandId           BIGINT IDENTITY(1,1) NOT NULL,
        StationId           BIGINT NOT NULL,
        SlotId              BIGINT NULL,
        SwapItemId          BIGINT NULL,
        ChargingSessionId   BIGINT NULL,
        CommandType         VARCHAR(30) NOT NULL,
        Payload             NVARCHAR(MAX) NULL,
        Status              VARCHAR(20) NOT NULL,
        CreatedAt           DATETIME2(3) NOT NULL,
        SentAt              DATETIME2(3) NULL,
        CompletedAt         DATETIME2(3) NULL,
        RetryCount          TINYINT NOT NULL
            CONSTRAINT DF_StationCommand_RetryCount DEFAULT (0),

        CONSTRAINT PK_StationCommand PRIMARY KEY CLUSTERED (CommandId),
        CONSTRAINT FK_StationCommand_Station
            FOREIGN KEY (StationId) REFERENCES dbo.Station(StationId),
        CONSTRAINT FK_StationCommand_Slot
            FOREIGN KEY (SlotId) REFERENCES dbo.Slot(SlotId),
        CONSTRAINT FK_StationCommand_SwapItem
            FOREIGN KEY (SwapItemId) REFERENCES dbo.SwapItem(SwapItemId),
        CONSTRAINT FK_StationCommand_ChargingSession
            FOREIGN KEY (ChargingSessionId) REFERENCES dbo.ChargingSession(ChargingSessionId),
        CONSTRAINT CK_StationCommand_Status
            CHECK (Status IN ('PENDING', 'SENT', 'ACKNOWLEDGED', 'SUCCEEDED', 'FAILED', 'TIMEOUT', 'CANCELLED')),
        CONSTRAINT CK_StationCommand_RetryCount CHECK (RetryCount >= 0),
        CONSTRAINT CK_StationCommand_Timestamps
            CHECK
            (
                (SentAt IS NULL OR SentAt >= CreatedAt)
                AND (CompletedAt IS NULL OR CompletedAt >= CreatedAt)
                AND (SentAt IS NULL OR CompletedAt IS NULL OR CompletedAt >= SentAt)
            )
    );

    CREATE TABLE dbo.StationEvent
    (
        EventId             BIGINT IDENTITY(1,1) NOT NULL,
        StationId           BIGINT NOT NULL,
        CommandId           BIGINT NULL,
        SlotId              BIGINT NULL,
        BatteryId           BIGINT NULL,
        EventIdentifier     VARCHAR(100) NOT NULL,
        EventType           VARCHAR(40) NOT NULL,
        Payload             NVARCHAR(MAX) NULL,
        OccurredAt          DATETIME2(3) NOT NULL,
        ReceivedAt          DATETIME2(3) NOT NULL,
        ProcessingStatus    VARCHAR(20) NOT NULL,
        ProcessedAt         DATETIME2(3) NULL,
        ProcessingError     NVARCHAR(255) NULL,

        CONSTRAINT PK_StationEvent PRIMARY KEY CLUSTERED (EventId),
        CONSTRAINT UQ_StationEvent_Station_EventIdentifier UNIQUE (StationId, EventIdentifier),
        CONSTRAINT FK_StationEvent_Station
            FOREIGN KEY (StationId) REFERENCES dbo.Station(StationId),
        CONSTRAINT FK_StationEvent_Command
            FOREIGN KEY (CommandId) REFERENCES dbo.StationCommand(CommandId),
        CONSTRAINT FK_StationEvent_Slot
            FOREIGN KEY (SlotId) REFERENCES dbo.Slot(SlotId),
        CONSTRAINT FK_StationEvent_Battery
            FOREIGN KEY (BatteryId) REFERENCES dbo.Battery(BatteryId),
        CONSTRAINT CK_StationEvent_ProcessingStatus
            CHECK (ProcessingStatus IN ('PENDING', 'PROCESSED', 'FAILED')),
        CONSTRAINT CK_StationEvent_ProcessingTime
            CHECK
            (
                (ProcessingStatus = 'PENDING' AND ProcessedAt IS NULL)
                OR
                (ProcessingStatus IN ('PROCESSED', 'FAILED') AND ProcessedAt IS NOT NULL)
            )
    );

    CREATE TABLE dbo.Alert
    (
        AlertId                 BIGINT IDENTITY(1,1) NOT NULL,
        StationId               BIGINT NOT NULL,
        EventId                 BIGINT NULL,
        AlertType               VARCHAR(40) NOT NULL,
        Severity                VARCHAR(20) NOT NULL,
        Title                   NVARCHAR(150) NOT NULL,
        Description             NVARCHAR(500) NULL,
        Status                  VARCHAR(20) NOT NULL,
        CreatedAt               DATETIME2(3) NOT NULL,
        AcknowledgedAt          DATETIME2(3) NULL,
        AcknowledgedByUserId    BIGINT NULL,
        ResolvedAt              DATETIME2(3) NULL,
        ResolvedByUserId        BIGINT NULL,

        CONSTRAINT PK_Alert PRIMARY KEY CLUSTERED (AlertId),
        CONSTRAINT FK_Alert_Station
            FOREIGN KEY (StationId) REFERENCES dbo.Station(StationId),
        CONSTRAINT FK_Alert_Event
            FOREIGN KEY (EventId) REFERENCES dbo.StationEvent(EventId),
        CONSTRAINT FK_Alert_AcknowledgedByUser
            FOREIGN KEY (AcknowledgedByUserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT FK_Alert_ResolvedByUser
            FOREIGN KEY (ResolvedByUserId) REFERENCES dbo.Users(UserId),
        CONSTRAINT CK_Alert_Severity
            CHECK (Severity IN ('INFO', 'WARNING', 'ERROR', 'CRITICAL')),
        CONSTRAINT CK_Alert_Status
            CHECK (Status IN ('OPEN', 'ACKNOWLEDGED', 'RESOLVED')),
        CONSTRAINT CK_Alert_Status_Assignment
            CHECK
            (
                (
                    Status = 'OPEN'
                    AND AcknowledgedAt IS NULL
                    AND AcknowledgedByUserId IS NULL
                    AND ResolvedAt IS NULL
                    AND ResolvedByUserId IS NULL
                )
                OR
                (
                    Status = 'ACKNOWLEDGED'
                    AND AcknowledgedAt IS NOT NULL
                    AND AcknowledgedByUserId IS NOT NULL
                    AND ResolvedAt IS NULL
                    AND ResolvedByUserId IS NULL
                )
                OR
                (
                    Status = 'RESOLVED'
                    AND AcknowledgedAt IS NOT NULL
                    AND AcknowledgedByUserId IS NOT NULL
                    AND ResolvedAt IS NOT NULL
                    AND ResolvedByUserId IS NOT NULL
                )
            ),
        CONSTRAINT CK_Alert_Timestamps
            CHECK
            (
                (AcknowledgedAt IS NULL OR AcknowledgedAt >= CreatedAt)
                AND (ResolvedAt IS NULL OR ResolvedAt >= CreatedAt)
                AND (AcknowledgedAt IS NULL OR ResolvedAt IS NULL OR ResolvedAt >= AcknowledgedAt)
            )
    );

    /* Supporting indexes for common FK-based lookups.
       SQL Server does not automatically index foreign-key columns. */

    CREATE NONCLUSTERED INDEX IX_Vehicle_UserId
        ON dbo.Vehicle(UserId);

    CREATE NONCLUSTERED INDEX IX_BatteryAssignment_VehicleId
        ON dbo.BatteryAssignment(VehicleId);

    CREATE NONCLUSTERED INDEX IX_BatteryAssignment_RecordedByUserId
        ON dbo.BatteryAssignment(RecordedByUserId)
        WHERE RecordedByUserId IS NOT NULL;

    CREATE NONCLUSTERED INDEX IX_ChargingSession_SlotId
        ON dbo.ChargingSession(SlotId);

    CREATE NONCLUSTERED INDEX IX_SwapSession_StationId
        ON dbo.SwapSession(StationId);

    CREATE NONCLUSTERED INDEX IX_SwapItem_ReturnedBatteryId
        ON dbo.SwapItem(ReturnedBatteryId)
        WHERE ReturnedBatteryId IS NOT NULL;

    CREATE NONCLUSTERED INDEX IX_SwapItem_IssuedBatteryId
        ON dbo.SwapItem(IssuedBatteryId)
        WHERE IssuedBatteryId IS NOT NULL;

    CREATE NONCLUSTERED INDEX IX_StationCommand_Station_CreatedAt
        ON dbo.StationCommand(StationId, CreatedAt DESC);

    CREATE NONCLUSTERED INDEX IX_StationEvent_Station_ReceivedAt
        ON dbo.StationEvent(StationId, ReceivedAt DESC);

    CREATE NONCLUSTERED INDEX IX_Alert_Station_Status_CreatedAt
        ON dbo.Alert(StationId, Status, CreatedAt DESC);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
