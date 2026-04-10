

public static class ReportMapping
{
    public static Report ToReport(this PostReportRequest request, int districtId, int? accountId)
    {
        return new Report(
            Is_Fixed: request.Is_Fixed,
            Problem_Category_id: request.Problem_Category_Id,
            Reported_District_id: districtId,
            Base_Account_id: accountId
        );
    }

    public static PostReportResponse ToPostReportResponse(this Report report)
    {
        return new PostReportResponse(
            Report_Id: report.Id,
            utc_Date_Report: report.UTC_ReportDate,
            Is_Fixed: report.IsFixed,
            Problem_Category_Id: report.ProblemCategoryId,
            Reported_District_Id: report.ReportedDistrictId,
            Account_Id: report.AccountId 
        );
    }
}