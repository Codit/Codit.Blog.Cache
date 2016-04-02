namespace Codit.Blog.Cache.Interfaces
{
  public interface ICacheKey
    {
        /// <summary>
        /// Name of the key in the cache
        /// </summary>
        string Name { get; }
    }
}