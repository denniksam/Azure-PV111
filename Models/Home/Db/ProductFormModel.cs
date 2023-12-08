namespace Azure_PV111.Models.Home.Db
{
    public class ProductFormModel
    {
        public Guid producerId { get; set; }
        public String name { get; set; } = null!;
        public int year { get; set; }
    }
}
