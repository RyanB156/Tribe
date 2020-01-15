using System;

namespace Tribe
{
    public class GameObjectEventArgs : EventArgs
    {
        public readonly GameObject gameObject;
        public GameObjectEventArgs(GameObject gameObject) { this.gameObject = gameObject; }
    }

    public class GameObjectController
    {
        public delegate void GameObjectHandler(object sender, GameObjectEventArgs e);
        public event GameObjectHandler AddGameObject;
        public event GameObjectHandler RemoveGameObject;

        protected virtual void OnAddGameObject(GameObject gameObject)
        {
            GameObjectHandler handler = AddGameObject;
            handler?.Invoke(this, new GameObjectEventArgs(gameObject));
        }

        protected virtual void OnRemoveGameObject(GameObject gameObject)
        {
            GameObjectHandler handler = RemoveGameObject;
            handler?.Invoke(this, new GameObjectEventArgs(gameObject));
        }
        
    }
}
