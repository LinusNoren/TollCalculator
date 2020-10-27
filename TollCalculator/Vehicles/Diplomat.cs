namespace TollCalculator.Vehicles
{
    public class Diplomat : IVehicle
    {
        public string GetVehicleType() => nameof(Diplomat);

        public bool TollFree() => true;
    }
}