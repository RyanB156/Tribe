using System;
using System.Collections.Generic;

namespace Tribe
{
    public class CraftingComponent
    {
        public readonly ItemType Type;
        public readonly int Amount;

        public CraftingComponent(ItemType type, int amount)
        {
            Type = type;
            Amount = amount;
        }

        public override string ToString()
        {
            return string.Format($"{Type}: {Amount}");
        }
    }

    public class CraftingMenu : IGetData
    {

        public int Index { get; private set; } = 0;
        public List<Tuple<CraftingComponent[], ItemType>> Recipes { get; private set; }

        private ObjectData data;

        public event DataChangedHandler UpdateElement;
        public event EventHandler CancelData;

        public CraftingMenu()
        {
            Recipes = new List<Tuple<CraftingComponent[], ItemType>>
            {
                // Crafting recipe for a spear.
                new Tuple<CraftingComponent[], ItemType>(new CraftingComponent[] {
                new CraftingComponent(ItemType.Stick, 2),
                new CraftingComponent(ItemType.Rock, 1)
                }, ItemType.Spear)
            };

            UpdateData();

            // TODO: More here...
        }

        public void DecrementItemIndex()
        {
            Index--;
            if (Index < 0)
                Index = Recipes.Count - 1;
        }

        public void IncrementItemIndex()
        {
            Index++;
            Index %= Recipes.Count;
        }

        // Data will be ("<Item name>", {ingredient list}) pairs.

        public void UpdateData()
        {
            data = new ObjectData(new List<Tuple<string, object>>(), 0);
            foreach (var recipe in Recipes)
            {
                // Add the name of the item as the field and the list of ingredients as the value.
                data.DataList.Add(new Tuple<string, object>(recipe.Item2.ToString(), recipe.Item1.CollectionToString()));
            }
        }

        public ObjectData GetData()
        {
            return data;
        }

        public int GetItemIndex()
        {
            return Index;
        }
    }
}
