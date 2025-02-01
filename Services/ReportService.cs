using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KnowledgeFlowApi.Data;
using KnowledgeFlowApi.DTOs;
using KnowledgeFlowApi.Entities;
using KnowledgeFlowApi.Enums;
using LibraryManagementSystemAPI.Services.SendEmailServices;
using Microsoft.EntityFrameworkCore;

public class ReportService
{
    private readonly ApplicationDbContext _context;
    private readonly SendEmailService _sendEmailService;

    public ReportService(ApplicationDbContext context, SendEmailService sendEmailService)
    {
        _context = context;
        _sendEmailService = sendEmailService;
    }

    public async Task<GetReportDto> SubmitReportAsync(SubmitReportDto submitReportDto)
    {
        var report = new Report
        {
            ReportedByUserId = submitReportDto.ReportedByUserId,
            ReportedUserId = submitReportDto.ReportedUserId,
            ReportedFileItemId = submitReportDto.ReportedFileItemId,
            Reason = submitReportDto.Reason,
            ReportDate = DateTime.UtcNow,
            Status = ReportStatus.Pending
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        
        await NotifyAdminsAsync(report);

        return MapToGetReportDto(report);
    }

    private async Task NotifyAdminsAsync(Report report)
    {
        var adminEmails = _context.Users.Where(u => u.Role == Role.Admin).Select(u => u.Email);

        var subject = "New Report Submitted";
        var body = $"A new report has been submitted:\n\n" +
                   $"Report ID: {report.Id}\n" +
                   $"Reason: {report.Reason}\n" +
                   $"Reported By User ID: {report.ReportedByUserId}\n" +
                   $"Reported User ID: {report.ReportedUserId}\n" +
                   $"Reported File ID: {report.ReportedFileItemId}\n" +
                   $"Date: {report.ReportDate}";

        foreach (var email in adminEmails)
        {
            await _sendEmailService.SendEmailAsync(email, subject, body);
        }
    }

    public async Task<List<GetReportDto>> GetReportsAsync(ReportStatus? status = null)
    {
        var query = _context.Reports.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        return query.Select(MapToGetReportDto).ToList();
    }

    public async Task<GetReviewReportDto> ReviewReportAsync(ReviewReportDto reviewReportDto)
    {
        var report = await _context.Reports
            .Include(r => r.ReportedUser)
            .Include(r => r.ReportedFileItem)
            .FirstOrDefaultAsync(r => r.Id == reviewReportDto.ReportId);

        if (report == null)
        {
            return new GetReviewReportDto { Message = "Report not found." };
        }
        
        if (report.Status == ReportStatus.Reviewed)
        {
            return new GetReviewReportDto { Message = "Report is Reviewed." };
        }

        report.Status = ReportStatus.Reviewed;

        switch (reviewReportDto.AdminAction)
        {
            case AdminAction.BanUser:
                if (report.ReportedUserId == null)
                {
                    return new GetReviewReportDto { Message = "Cannot ban a user for a file report." };
                }
                await HandleUserBanAsync(report.ReportedUserId, reviewReportDto.ViolationType, reviewReportDto.AdminComment);
                break;

            case AdminAction.DeleteFile:
                if (report.ReportedFileItemId == null)
                {
                    return new GetReviewReportDto { Message = "Cannot delete a file for a user report." };
                }
                await HandleUserBanAsync(report.ReportedUserId, reviewReportDto.ViolationType, reviewReportDto.AdminComment);
                await DeleteFileAsync((int)report.ReportedFileItemId);
                break;

            case AdminAction.IncorrectReport:
                return new GetReviewReportDto { Message = "Incorrect Report" };
            
            default:
                return new GetReviewReportDto { Message = "Incorrect Report" };
        }

        await _context.SaveChangesAsync();

        return new GetReviewReportDto 
        {
            IsValid = true,
            AdminAction = reviewReportDto.AdminAction
        };
    }

    private async Task HandleUserBanAsync(int userId, ViolationType violation, string adminComment)
    {
        var violations = await _context.UserViolations
            .CountAsync(v => v.UserId == userId);

        if (violations < 2)
        {
            await AddUserViolationAsync(userId, violation);
        }
        else if (violations == 2)
        {
            // Ban for one month
            var ban = new Ban
            {
                UserId = userId,
                BanStartDate = DateTime.UtcNow,
                BanEndDate = DateTime.UtcNow.AddMonths(1),
                AdminComment = adminComment
            };
            _context.Bans.Add(ban);
        }
        else
        {
            // Permanent ban
            var ban = new Ban
            {
                UserId = userId,
                BanStartDate = DateTime.UtcNow,
                AdminComment = adminComment
            };
            _context.Bans.Add(ban);
        }

        await _context.SaveChangesAsync();
    }

    // Add a user violation
    private async Task AddUserViolationAsync(int userId, ViolationType violationType)
    {
        var violation = new UserViolation
        {
            UserId = userId,
            ViolationType = violationType,
            ViolationDate = DateTime.UtcNow,
        };

        _context.UserViolations.Add(violation);
        await _context.SaveChangesAsync();
    }

    private async Task DeleteFileAsync(int fileItemId)
    {
        var file = await _context.FileItems.FindAsync(fileItemId);
        if (file != null)
        {
            _context.FileItems.Remove(file);
            await _context.SaveChangesAsync();
        }
    }

    public bool IsBanned(int userId)
    {
        var ban = _context.Bans
            .OrderByDescending(b => b.BanStartDate)
            .FirstOrDefault(u => u.UserId == userId);

        if (ban == null)
            return false;

        if (ban.BanEndDate == null || ban.BanEndDate > DateTime.UtcNow)
            return true;

        return false;
    }

    private GetReportDto MapToGetReportDto(Report report) {
        return new GetReportDto
        {
            ReportedByUserId = report.ReportedByUserId,
            ReportedUserId = report.ReportedUserId,
            ReportedFileItemId = report.ReportedFileItemId,
            ReportStatus = ReportStatus.Pending,
            ReportDate = DateTime.UtcNow,
            Reason = report.Reason,
            IsValid = true
        };
    }
}