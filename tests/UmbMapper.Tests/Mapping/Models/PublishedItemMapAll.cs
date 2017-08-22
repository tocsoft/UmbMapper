namespace UmbMapper.Tests.Mapping.Models
{
    public class PublishedItemMapAll : ClassMap<PublishedItem>
    {
        public PublishedItemMapAll()
        {
            this.AllProperties();
        }
    }
}
