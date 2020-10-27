namespace TollCalculator.Vehicles
{
    public class Truck : IVehicle
    {
        public string GetVehicleType() => nameof(Truck);

        public bool TollFree() => true;
    }
}