public record GetCityStatisticsResponse(
    string City_Name,
    Dictionary<int, DistrictStatistics> Districts_Data //[District_Id] --> (District_Name + Problems List ) 
);

public record DistrictStatistics(
    string District_Name,
    List<DistrictSingleStatistic> District_Statistics
);

public record DistrictSingleStatistic(
    int Problem_Category_Id,
    string Problem_Category_Name,
    int reported_Amount
);

// -> string "City_Name"
// -> [ int District_ID ]
//       -> string "District_Name"  
//       -> [
//           -> int Problem_Category_Id
//           -> string "Problem_Category_Name"
//           -> int reportsAmount
//              ,
//              [...]
//          ] 
//    [...]

