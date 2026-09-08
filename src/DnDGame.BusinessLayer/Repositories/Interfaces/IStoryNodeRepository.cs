using DnDGame.Domain.Entities.Game;

namespace DnDGame.BusinessLayer.Repositories.Interfaces;

/// <summary>
/// Direct access to a single StoryNode (with its Choices already attached), used while
/// a GameSession is walking the story graph one step at a time, without needing to
/// reload the whole Adventure aggregate on every choice.
/// </summary>
public interface IStoryNodeRepository
{
    Task<StoryNode?> GetByIdAsync(int id);
}
