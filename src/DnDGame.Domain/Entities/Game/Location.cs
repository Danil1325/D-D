using DnDGame.Domain.Common;

namespace DnDGame.Domain.Entities.Game;

public class Location : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
