using Island.Game.Proxy.Items;
using Island.Game.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Proxy.Recipes
{
    public abstract class RecipeBase : IRecipe
    {
        public string Name => ToString();
        public IItem Result { get; private set; }

        public static RecipeBase Get(string tool, RecipeType type, string[] items)
        {
            if (GameManager.ProxyManager.TryGet<IRecipe>(GetRecipeName(tool, type, items), out var proxy))
                return (RecipeBase) proxy;
            return null;
        }

        private static string GetRecipeName(string tool, RecipeType type, string[] items)
        {
            var recipeName = tool + "(";

            if (type == RecipeType.NonOrdered)
                Array.Sort(items);

            foreach (var item in items)
                recipeName += item + ",";

            recipeName += ")";
            recipeName += type.ToString();

            return "island.recipe:" + recipeName;
        }

        public override string ToString()
        {
            return GetRecipeName(GetTool(), GetRecipeType(), GetRecipeItems());
        }

        public void Init()
        {
            Result = GameManager.ProxyManager.Get<IItem>(GetResult());
        }

        public abstract string GetTool();
        public abstract string[] GetRecipeItems();
        public abstract RecipeType GetRecipeType();
        public abstract string GetResult();
    }
}
