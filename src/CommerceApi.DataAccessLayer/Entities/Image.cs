using CommerceApi.DataAccessLayer.Entities.Common;

namespace CommerceApi.DataAccessLayer.Entities;

public class Image : FileEntity
{
    public string Title { get; set; }

    public string Description { get; set; }
}