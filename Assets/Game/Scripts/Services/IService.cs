namespace Game.Scripts.Services
{
    public interface IService
    {
        public void LocalAwake();
        public void LocalStart();
        public void LocalUpdate(float deltaTime);

    }
}