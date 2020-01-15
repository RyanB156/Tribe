namespace Tribe
{
    class DDeltaConfig
    {
        public static readonly double attackerResponseDelta = 0.05;

        public static readonly double idleActionAdvertisement = 0.002;

        public static readonly double sleepActionAdvertisement = 0.1;
        public static readonly double sleepHealthAdvertisement = 1.0;
        public static readonly double napActionDelta = 0.0005;

        public static readonly double guardBaseDelta = 0.001;

        public static readonly double consumeFoodDelta = 0.35;
        public static readonly double dropFoodDelta = 0.125;
        public static readonly double harvestPlantDelta = 0.15;
    }
}
