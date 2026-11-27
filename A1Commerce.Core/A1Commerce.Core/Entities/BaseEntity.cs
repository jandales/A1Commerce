namespace A1Commerce.Core.Entities
{
    public class BaseEntity
    {
        public int id { get; set; }
        public int? intConcurrencyId {get;set;}
        public DateTime? CreateAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
