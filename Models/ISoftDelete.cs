namespace MigraTrackAPI.Models
{
    public interface ISoftDelete
    {
        int IsDeletedTransaction { get; set; }
    }
}
