namespace TollCalculator.Vehicles
{
    public class Foreign : IVehicle
    {
        public string GetVehicleType() => nameof(Foreign);

        public bool TollFree() => true;
    }
}