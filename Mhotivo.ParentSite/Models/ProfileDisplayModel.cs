namespace Mhotivo.ParentSite.Models
{
    public class ProfileDisplayModel
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public byte[] Photo { get; set; }
        public string Description { get; set; }
    }
}