

public static class DistrictMapping
{
    public static GetDistrictsResponse ToGetDistrictsResponse(List<District> districts)
    {
        if(districts is null || districts.Count <= 0)
        {
            throw new InvalidDataException("List is empty or Doesnt exist");
        }

        int cityId = districts[0].City!.Id;
        Dictionary<int, string> idToDistrict = new();

        foreach(District d in districts)
        {
            idToDistrict.Add(d.Id, d.Name);
        }

        return new GetDistrictsResponse(cityId, idToDistrict);
    }
}