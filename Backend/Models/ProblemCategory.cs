public class ProblemCategory
{
        public int Id {get; set;} = -1;
        public string Name {get; set;} = "unnamed";


    public ProblemCategory(long Problem_Category_id, string Problem_Category_Name)
    {
        this.Id = (int)Problem_Category_id;
        this.Name = Problem_Category_Name;
    }
}