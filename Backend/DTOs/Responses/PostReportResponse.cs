public record PostReportResponse(
    int Report_id,
    long utc_Date_Report,
    int Problem_Category_id,
    int Reported_District_id,
    int? Account_id 
);