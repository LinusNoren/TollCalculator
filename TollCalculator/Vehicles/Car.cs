namespace TollCalculator.Vehicles
{
    public class Car : IVehicle
    {
        public string GetVehicleType() => nameof(Car);

        public bool TollFree() => false;
    }
}