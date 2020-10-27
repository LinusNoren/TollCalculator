namespace TollCalculator.Vehicles
{
    public class Motorbike : IVehicle
    {
        public string GetVehicleType() => nameof(Motorbike);

        public bool TollFree() => true;
    }
}
