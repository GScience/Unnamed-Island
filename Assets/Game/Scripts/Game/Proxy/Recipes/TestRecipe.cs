using Island.Game.EntityBehaviour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Island.Game.Proxy.Recipes
{
    class TestRecipe : RecipeBase
    {
        public override string[] GetRecipeItems()
        {
            return new string[]
            {
                "island.item:fresh_grass","island.item:fresh_grass","island.item:fresh_grass",
                "","","",
                "","",""
            };
        }

        public override RecipeType GetRecipeType()
        {
            return RecipeType.Ordered;
        }

        public override string GetResult()
        {
            return "island.item:stone";
        }

        public override string GetTool()
        {
            return CraftTableBehaviour.ToolName;
        }
    }
}
