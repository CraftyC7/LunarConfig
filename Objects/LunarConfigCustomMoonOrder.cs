
using System.Collections.Generic;
using System.Linq;
using Dawn;

namespace LunarConfig.Objects.Config
{
    public class LunarConfigCustomMoonOrder(Dictionary<DawnMoonInfo, int> newCatalogueIndex) : IMoonOrderingStep
    {
        public IOrderedEnumerable<DawnMoonInfo> ApplyInitial(IEnumerable<DawnMoonInfo> input, bool reverse = false)
        {
            return reverse
                ? input.OrderBy(GetIndex)
                : input.OrderByDescending(GetIndex);
        }

        public IOrderedEnumerable<DawnMoonInfo> ApplyNext(IOrderedEnumerable<DawnMoonInfo> input, bool reverse = false)
        {
            return reverse
                ? input.ThenBy(GetIndex)
                : input.ThenByDescending(GetIndex);
        }

        private int GetIndex(DawnMoonInfo moonInfo) => newCatalogueIndex[moonInfo];
    }
}