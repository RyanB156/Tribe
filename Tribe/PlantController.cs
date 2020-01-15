using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Tribe
{
    class PlantController : GameObjectController
    {
        private readonly double spawnChance = 0.0084; // 1 / (5 * 24 ticks per second).
        private readonly double appleSpawnChance = 0.015;
        public List<Plant> Plants { get; private set; }
        Stack<Plant> deadPlants = new Stack<Plant>();
        Renderer plantRenderer;

        public PlantController()
        {

            plantRenderer = new DefaultPictureRenderer();

        }

        public void CreateObjects(OrderedPair<int> center)
        {
            Plant potato = GeneralPlant.Potato(80, 80);
            List<Plant> yuccaPlants = new List<Plant>();
            Plant pineTree1 = GeneralPlant.PineTree(230, 400);
            List<Plant> appleTrees = new List<Plant>
            {
                new AppleTree(center.X + 50, center.Y + 120),
                new AppleTree(center.X + 180, center.Y + 50)
            };

            for (int i = 0;  i < 10; i++)
            {
                var p = Utilities.GetRandomPoint();
                appleTrees.Add(new AppleTree(p.X, p.Y));
            }

            for (int i = 0; i < 10; i++)
            {
                var p = Utilities.GetRandomPoint();
                yuccaPlants.Add(GeneralPlant.Yucca(p.X, p.Y));
            }

            List<Plant> plants = new List<Plant>() { potato, pineTree1 }
                .Concat(appleTrees)
                .Concat(yuccaPlants)
                .ToList();

            // Spawn shrubs in random locations.
            for (int i = 0; i < 20; i++)
            {
                OrderedPair<int> position = Utilities.GetRandomPoint();
                plants.Add(GeneralPlant.Shrub(position.X, position.Y));
            }


            Plants = new List<Plant>();
            foreach (Plant plant in plants)
            {
                AddPlant(plant);
            }
        }

        private void AddPlant(Plant plant)
        {
            OnAddGameObject(plant); // Let WorldController know that a new plant has been added and it needs to listen for schedule events.
            Plants.Add(plant);
            plant.DropItem += Plant_DropItem;
            plant.TriggerGrowth();
        }

        private void SpawnRandomPlant()
        {
            Plant p;
            OrderedPair<int> randomPoint = Utilities.GetRandomPoint();

            // TODO: Create a range of random values for different plants to generate. Then use the frequency selection method from UtilityDecider class
            // to pick the plant to spawn.
            if (Utilities.Rng.Next(0, 2) == 0)
                p = GeneralPlant.PineTree(randomPoint.X, randomPoint.Y);
            else
                p = GeneralPlant.Yucca(randomPoint.X, randomPoint.Y);

            AddPlant(p);
        }

        private void Plant_DropItem(object o, ItemEventArgs e)
        {
            OnAddGameObject(e.Item);
        }

        private void RemovePlant(Plant plant)
        {
            Plants.Remove(plant);
            OnRemoveGameObject(plant);
        }

        
        public void PlantUpdate(GameTime gameTime)
        {

            if (Utilities.Rng.NextDouble() < spawnChance)
                SpawnRandomPlant();

            foreach (Plant p in Plants)
            {
                // Plant has been destroyed or used up, so remove it.
                if (p.IsHarvested)
                {
                    p.OnCancelData();
                    deadPlants.Push(p);
                    continue;
                }

                p.CarryOutAction();

                if (p is AppleTree appleTree)
                {
                    if (Utilities.Rng.NextDouble() < appleSpawnChance)
                        appleTree.AddApple();
                }
            }
            while (deadPlants.Count > 0)
            {
                Plants.Remove(deadPlants.Pop());
            }
        }


        public void RenderPlants(Graphics graphics, double scale, OrderedPair<double> origin, OrderedPair<double> center)
        {
            foreach (Plant p in Plants)
            {
                plantRenderer.DrawRender(p.RenderContext, scale, origin, center, graphics);
            }
        }
    }
}
