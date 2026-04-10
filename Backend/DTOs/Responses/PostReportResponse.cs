public record PostReportResponse(
    int Report_Id,
    long utc_Date_Report,
    bool Is_Fixed,
    int Problem_Category_Id,
    int Reported_District_Id,
    int? Account_Id 
);