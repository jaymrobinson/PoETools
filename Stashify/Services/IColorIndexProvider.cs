using System.Collections.Generic;

namespace Stashify.Services
{
    public interface IColorIndexProvider
    {
        IEnumerable<ColorIndexElement> GetIndexElements();
    }
}