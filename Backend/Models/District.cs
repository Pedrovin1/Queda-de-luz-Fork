public class District
{
    public int Id {get; set;} = -1;
    public string Name {get; set;} = "unnamed";
    public City? City {get; set;}

    //public District(){}
    public District(long District_id, string District_Name, City City)
    {
        this.Id = (int)District_id;
        this.Name = District_Name;
        this.City = City;
    }

    public District(long District_id, string District_Name)
    {
        this.Id = (int)District_id;
        this.Name = District_Name;
    }
}