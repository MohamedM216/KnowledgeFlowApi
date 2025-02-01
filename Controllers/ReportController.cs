using KnowledgeFlowApi.Attributes;
using KnowledgeFlowApi.DTOs;
using KnowledgeFlowApi.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportController(ReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpPost("submit")]
    [BannedUser]
    [Authorize(Roles = Role.User)]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> SubmitReport([FromBody] SubmitReportDto submitReportDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var report = await _reportService.SubmitReportAsync(submitReportDto);
        return Ok(report);
    }

    [HttpGet]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> GetReports([FromQuery] ReportStatus? status)
    {
        var reports = await _reportService.GetReportsAsync(status);
        return Ok(reports);
    }

    [HttpPost("review")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> ReviewReport([FromBody] ReviewReportDto reviewReportDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _reportService.ReviewReportAsync(reviewReportDto);

        if (!result.IsValid)
        {
            return BadRequest(result.Message);
        }

        return Ok(result);
    }
}