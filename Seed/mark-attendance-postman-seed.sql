SET NOCOUNT ON;

DECLARE @Employee1Id UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @Employee2Id UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @OfficeId UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';

DECLARE @OfficeLat DECIMAL(9,6) = 30.044400;
DECLARE @OfficeLng DECIMAL(9,6) = 31.235700;

IF EXISTS (SELECT 1 FROM OfficeLocations WHERE Id = @OfficeId)
BEGIN
    UPDATE OfficeLocations
    SET Name = 'Postman Test Office',
        Latitude = @OfficeLat,
        Longitude = @OfficeLng,
        AllowedRadiusMeters = 100,
        IsActive = 1,
        CreatedAt = GETUTCDATE()
    WHERE Id = @OfficeId;
END
ELSE
BEGIN
    INSERT INTO OfficeLocations (Id, Name, Latitude, Longitude, AllowedRadiusMeters, IsActive, CreatedAt)
    VALUES (@OfficeId, 'Postman Test Office', @OfficeLat, @OfficeLng, 100, 1, GETUTCDATE());
END;

IF NOT EXISTS (SELECT 1 FROM Employees WHERE Id = @Employee1Id)
BEGIN
    INSERT INTO Employees (Id, FullName, Email, Phone, Department, Position, JoinDate, FaceImagePath, CreatedAt)
    VALUES (
        @Employee1Id,
        'Postman User One',
        'postman.user1@test.local',
        '+201000000001',
        'Engineering',
        'Backend Intern',
        CAST('2026-03-01' AS date),
        NULL,
        GETUTCDATE()
    );
END;

IF NOT EXISTS (SELECT 1 FROM Employees WHERE Id = @Employee2Id)
BEGIN
    INSERT INTO Employees (Id, FullName, Email, Phone, Department, Position, JoinDate, FaceImagePath, CreatedAt)
    VALUES (
        @Employee2Id,
        'Postman User Two',
        'postman.user2@test.local',
        '+201000000002',
        'HR',
        'HR Intern',
        CAST('2026-03-01' AS date),
        NULL,
        GETUTCDATE()
    );
END;

DECLARE @TodayUtc DATE = CAST(GETUTCDATE() AS date);
DELETE FROM Attendance
WHERE EmployeeId IN (@Employee1Id, @Employee2Id)
  AND MarkedAt >= @TodayUtc
  AND MarkedAt < DATEADD(DAY, 1, @TodayUtc);

SELECT 'employee1Uuid' AS [key], CAST(@Employee1Id AS NVARCHAR(36)) AS [value]
UNION ALL SELECT 'employee2Uuid', CAST(@Employee2Id AS NVARCHAR(36))
UNION ALL SELECT 'officeLatitude', CAST(@OfficeLat AS NVARCHAR(32))
UNION ALL SELECT 'officeLongitude', CAST(@OfficeLng AS NVARCHAR(32));
