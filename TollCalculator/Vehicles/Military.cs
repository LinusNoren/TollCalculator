namespace TollCalculator.Vehicles
{
    public class Military : IVehicle
    {
        public string GetVehicleType() => nameof(Military);

        public bool TollFree() => true;
    }
}