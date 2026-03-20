public record PostReportRequest(
    int Problem_Category_id,
    bool Is_Fixed = false,
    int? Account_id = null
);

