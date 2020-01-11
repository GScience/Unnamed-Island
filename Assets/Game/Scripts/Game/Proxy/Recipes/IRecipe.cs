using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Proxy.Recipes
{
    public enum RecipeType
    {
        NonOrdered, Ordered
    }

    public interface IRecipe : IProxy
    {
        // 合成工具
        string GetTool();

        // 合成材料
        string[] GetRecipeItems();

        // 合成类型
        RecipeType GetRecipeType();

        // 合成结果
        string GetResult();
    }
}
