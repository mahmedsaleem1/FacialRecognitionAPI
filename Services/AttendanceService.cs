using FacialRecognitionAPI.Configuration;
using FacialRecognitionAPI.Models.DTOs.Requests;
using FacialRecognitionAPI.Models.DTOs.Responses;
using FacialRecognitionAPI.Models.Entities;
using FacialRecognitionAPI.Repositories.Interfaces;
using FacialRecognitionAPI.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace FacialRecognitionAPI.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IFacialRecognitionService _faceService;
    private readonly IEncryptionService _encryptionService;
    private readonly AttendanceSettings _attendanceSettings;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(
        IAttendanceRepository attendanceRepo,
        IEmployeeRepository employeeRepo,
        IFacialRecognitionService faceService,
        IEncryptionService encryptionService,
        IOptions<AttendanceSettings> attendanceSettings,
        ILogger<AttendanceService> logger)
    {
        _attendanceRepo = attendanceRepo;
        _employeeRepo = employeeRepo;
        _faceService = faceService;
        _encryptionService = encryptionService;
        _attendanceSettings = attendanceSettings.Value;
        _logger = logger;
    }

    public async Task<MarkAttendanceResponse> MarkCheckInAsync(MarkAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        // Decode face image
        byte[] imageBytes;
        try
        {
            imageBytes = Convert.FromBase64String(request.FaceImageBase64);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Invalid Base64 face image data.");
        }

        // Extract embedding from captured image
        var capturedEmbedding = await _faceService.ExtractFaceEmbeddingAsync(imageBytes)
            ?? throw new InvalidOperationException("No face detected in the provided image. Please ensure a clear face is visible.");

        Employee? matchedEmployee;
        float bestSimilarity;

        if (request.EmployeeId.HasValue)
        {
            // 1:1 Verification — compare against specific employee
            matchedEmployee = await _employeeRepo.GetByIdAsync(request.EmployeeId.Value, cancellationToken)
                ?? throw new KeyNotFoundException($"Employee with ID {request.EmployeeId.Value} not found.");

            if (!matchedEmployee.IsActive)
                throw new InvalidOperationException("Employee is deactivated.");

            var storedEmbedding = _encryptionService.Decrypt(
                matchedEmployee.EncryptedEmbedding, matchedEmployee.EncryptionIv, matchedEmployee.EncryptionTag);

            bestSimilarity = _faceService.ComputeSimilarity(storedEmbedding, capturedEmbedding);

            if (!_faceService.VerifyFace(storedEmbedding, capturedEmbedding))
            {
                return new MarkAttendanceResponse
                {
                    Recognized = false,
                    SimilarityScore = bestSimilarity,
                    Message = "Face verification failed. The captured face does not match the employee's registered face."
                };
            }
        }
        else
        {
            // 1:N Search — find best match among all active employees
            (matchedEmployee, bestSimilarity) = await FindBestMatchAsync(capturedEmbedding, cancellationToken);

            if (matchedEmployee == null)
            {
                return new MarkAttendanceResponse
                {
                    Recognized = false,
                    SimilarityScore = bestSimilarity,
                    Message = "No matching employee found. The face is not registered in the system."
                };
            }
        }

        // Check if already checked in today
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var existingRecord = await _attendanceRepo.GetRecordAsync(matchedEmployee.Id, today, cancellationToken);

        if (existingRecord != null)
        {
            return new MarkAttendanceResponse
            {
                Recognized = true,
                EmployeeId = matchedEmployee.Id,
                EmployeeCode = matchedEmployee.EmployeeCode,
                EmployeeName = matchedEmployee.FullName,
                SimilarityScore = bestSimilarity,
                CheckInTime = existingRecord.CheckInTime.ToString("HH:mm:ss"),
                Status = existingRecord.Status.ToString(),
                Message = $"Already checked in today at {existingRecord.CheckInTime:HH:mm}."
            };
        }

        // Determine attendance status based on current time
        var now = TimeOnly.FromDateTime(DateTime.UtcNow);
        var status = DetermineAttendanceStatus(now);

        var record = new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            EmployeeId = matchedEmployee.Id,
            Date = today,
            CheckInTime = now,
            CheckInSimilarityScore = bestSimilarity,
            Status = status,
            Notes = status switch
            {
                AttendanceStatus.Late => $"Late by {(now - _attendanceSettings.WorkdayStart):hh\\:mm}",
                AttendanceStatus.HalfDay => $"Half-day: arrived at {now:HH:mm}",
                _ => null
            },
            CreatedAt = DateTime.UtcNow
        };

        await _attendanceRepo.AddAsync(record, cancellationToken);
        await _attendanceRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Attendance marked: {Code} ({Name}) checked in at {Time}, status={Status}, similarity={Score:F4}",
            matchedEmployee.EmployeeCode, matchedEmployee.FullName, now, status, bestSimilarity);

        return new MarkAttendanceResponse
        {
            Recognized = true,
            EmployeeId = matchedEmployee.Id,
            EmployeeCode = matchedEmployee.EmployeeCode,
            EmployeeName = matchedEmployee.FullName,
            SimilarityScore = bestSimilarity,
            CheckInTime = now.ToString("HH:mm:ss"),
            Status = status.ToString(),
            Message = $"Attendance marked successfully. Status: {status}."
        };
    }

    public async Task<MarkAttendanceResponse> MarkCheckOutAsync(CheckOutRequest request, CancellationToken cancellationToken = default)
    {
        byte[] imageBytes;
        try
        {
            imageBytes = Convert.FromBase64String(request.FaceImageBase64);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Invalid Base64 face image data.");
        }

        var capturedEmbedding = await _faceService.ExtractFaceEmbeddingAsync(imageBytes)
            ?? throw new InvalidOperationException("No face detected in the provided image.");

        Employee? matchedEmployee;
        float bestSimilarity;

        if (request.EmployeeId.HasValue)
        {
            matchedEmployee = await _employeeRepo.GetByIdAsync(request.EmployeeId.Value, cancellationToken)
                ?? throw new KeyNotFoundException($"Employee with ID {request.EmployeeId.Value} not found.");

            var storedEmbedding = _encryptionService.Decrypt(
                matchedEmployee.EncryptedEmbedding, matchedEmployee.EncryptionIv, matchedEmployee.EncryptionTag);

            bestSimilarity = _faceService.ComputeSimilarity(storedEmbedding, capturedEmbedding);

            if (!_faceService.VerifyFace(storedEmbedding, capturedEmbedding))
            {
                return new MarkAttendanceResponse
                {
                    Recognized = false,
                    SimilarityScore = bestSimilarity,
                    Message = "Face verification failed at check-out."
                };
            }
        }
        else
        {
            (matchedEmployee, bestSimilarity) = await FindBestMatchAsync(capturedEmbedding, cancellationToken);

            if (matchedEmployee == null)
            {
                return new MarkAttendanceResponse
                {
                    Recognized = false,
                    SimilarityScore = bestSimilarity,
                    Message = "No matching employee found."
                };
            }
        }

        // Find today's attendance record
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var record = await _attendanceRepo.GetRecordAsync(matchedEmployee.Id, today, cancellationToken)
            ?? throw new InvalidOperationException($"No check-in record found for {matchedEmployee.FullName} today. Please check in first.");

        if (record.CheckOutTime.HasValue)
        {
            return new MarkAttendanceResponse
            {
                Recognized = true,
                EmployeeId = matchedEmployee.Id,
                EmployeeCode = matchedEmployee.EmployeeCode,
                EmployeeName = matchedEmployee.FullName,
                SimilarityScore = bestSimilarity,
                Message = $"Already checked out today at {record.CheckOutTime:HH:mm}."
            };
        }

        var now = TimeOnly.FromDateTime(DateTime.UtcNow);
        record.CheckOutTime = now;
        record.CheckOutSimilarityScore = bestSimilarity;

        _attendanceRepo.Update(record);
        await _attendanceRepo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Check-out: {Code} ({Name}) at {Time}", matchedEmployee.EmployeeCode, matchedEmployee.FullName, now);

        return new MarkAttendanceResponse
        {
            Recognized = true,
            EmployeeId = matchedEmployee.Id,
            EmployeeCode = matchedEmployee.EmployeeCode,
            EmployeeName = matchedEmployee.FullName,
            SimilarityScore = bestSimilarity,
            CheckInTime = record.CheckInTime.ToString("HH:mm:ss"),
            Status = record.Status.ToString(),
            Message = $"Checked out at {now:HH:mm}."
        };
    }

    public async Task<PagedResult<AttendanceResponse>> GetAttendanceRecordsAsync(AttendanceQueryParams query, CancellationToken cancellationToken = default)
    {
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);
        query.Page = Math.Max(1, query.Page);

        var (total, items) = await _attendanceRepo.GetPagedAsync(
            query.From, query.To, query.Department, query.EmployeeId,
            query.Page, query.PageSize, cancellationToken);

        return new PagedResult<AttendanceResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<List<AttendanceResponse>> GetTodayAttendanceAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var records = await _attendanceRepo.GetByDateWithEmployeeAsync(today, cancellationToken);
        return records.Select(MapToResponse).ToList();
    }

    public async Task<AttendanceResponse?> GetEmployeeTodayAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var record = await _attendanceRepo.GetTodayRecordAsync(employeeId, cancellationToken);
        if (record == null) return null;

        // Load employee for mapping
        var employee = await _employeeRepo.GetByIdAsync(employeeId, cancellationToken);
        if (employee != null)
            record.Employee = employee;

        return MapToResponse(record);
    }

    #region Private Helpers

    /// <summary>
    /// 1:N face search — finds the best-matching employee from all active employees.
    /// Uses parallel decryption and SIMD-optimized cosine similarity for performance.
    /// </summary>
    private async Task<(Employee? Employee, float Similarity)> FindBestMatchAsync(float[] capturedEmbedding, CancellationToken cancellationToken)
    {
        var employees = await _employeeRepo.GetAllActiveWithEmbeddingsAsync(cancellationToken);

        if (employees.Count == 0)
            return (null, 0f);

        Employee? bestMatch = null;
        float bestSimilarity = float.MinValue;
        var lockObj = new object();

        // Parallel comparison for performance with large employee sets
        Parallel.ForEach(employees, employee =>
        {
            try
            {
                var storedEmbedding = _encryptionService.Decrypt(
                    employee.EncryptedEmbedding, employee.EncryptionIv, employee.EncryptionTag);

                var similarity = _faceService.ComputeSimilarity(storedEmbedding, capturedEmbedding);

                lock (lockObj)
                {
                    if (similarity > bestSimilarity)
                    {
                        bestSimilarity = similarity;
                        bestMatch = employee;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to compare face for employee {Code}", employee.EmployeeCode);
            }
        });

        // Check if the best match meets the verification threshold
        if (bestMatch != null && _faceService.VerifyFace(
            _encryptionService.Decrypt(bestMatch.EncryptedEmbedding, bestMatch.EncryptionIv, bestMatch.EncryptionTag),
            capturedEmbedding))
        {
            _logger.LogInformation("Face matched: {Code} ({Name}), similarity={Score:F4}",
                bestMatch.EmployeeCode, bestMatch.FullName, bestSimilarity);
            return (bestMatch, bestSimilarity);
        }

        _logger.LogInformation("No face match found. Best similarity: {Score:F4}", bestSimilarity);
        return (null, bestSimilarity);
    }

    private AttendanceStatus DetermineAttendanceStatus(TimeOnly checkInTime)
    {
        var deadline = _attendanceSettings.WorkdayStart.AddMinutes(_attendanceSettings.GraceMinutes);
        var halfDayDeadline = _attendanceSettings.WorkdayStart.AddMinutes(_attendanceSettings.HalfDayThresholdMinutes);

        if (checkInTime <= deadline)
            return AttendanceStatus.Present;

        if (checkInTime <= halfDayDeadline)
            return AttendanceStatus.Late;

        return AttendanceStatus.HalfDay;
    }

    private static AttendanceResponse MapToResponse(AttendanceRecord record) => new()
    {
        Id = record.Id,
        EmployeeId = record.EmployeeId,
        EmployeeCode = record.Employee?.EmployeeCode ?? string.Empty,
        EmployeeName = record.Employee?.FullName ?? string.Empty,
        Department = record.Employee?.Department,
        Date = record.Date,
        CheckInTime = record.CheckInTime,
        CheckOutTime = record.CheckOutTime,
        CheckInSimilarityScore = record.CheckInSimilarityScore,
        CheckOutSimilarityScore = record.CheckOutSimilarityScore,
        Status = record.Status,
        Notes = record.Notes
    };

    #endregion
}
