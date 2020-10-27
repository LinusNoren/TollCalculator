namespace TollCalculator.Vehicles
{
    public class Emergency : IVehicle
    {
        public string GetVehicleType() => nameof(Emergency);

        public bool TollFree() => true;
    }
}