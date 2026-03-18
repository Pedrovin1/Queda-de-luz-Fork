public static class ReportMapping
{
    public static Report ToReport(this PostReportRequest request, int districtId)
    {
        return new Report(
            Problem_Category_id: request.Problem_Category_id,
            Reported_District_id: districtId,
            Base_Account_id: request.Account_id ?? null
        );
    }

    public static PostReportResponse ToPostReportResponse(this Report report)
    {
        return new PostReportResponse(
            Report_id: report.Id,
            utc_Date_Report: report.UTC_ReportDate,
            Problem_Category_id: report.ProblemCategoryId,
            Reported_District_id: report.ReportedDistrictId,
            Account_id: report.AccountId 
        );
    }
}