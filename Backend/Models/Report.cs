public class Report
{
    public int Id {get; set;}
    public long UTC_ReportDate {get; set;}
    public int ProblemCategoryId {get; set;}
    public int ReportedDistrictId {get; set;}  
    public int? AccountId {get; set;}

    //To recover from the Database
    public Report(long Report_id, long UTC_Date_Report, long Problem_Category_id, long Reported_District_id, long? Base_Account_id)
    {
        this.Id = (int)Report_id;
        this.UTC_ReportDate = UTC_Date_Report;
        this.ProblemCategoryId = (int)Problem_Category_id;
        this.ReportedDistrictId = (int)Reported_District_id;
        this.AccountId = (int?) Base_Account_id;
    }

    //To insert into the database
    public Report(long Problem_Category_id, long Reported_District_id, long? Base_Account_id)
    {
        this.ProblemCategoryId = (int)Problem_Category_id;
        this.ReportedDistrictId = (int)Reported_District_id;
        this.AccountId = (int?) Base_Account_id;
    }

}