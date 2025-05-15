namespace CheckCarsAPI.Models
{
    public class CarService
    {
        public int CarServiceId { get; set; }
        public string? Title { get; set; }
        public DateTime Date { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public int mileage { get; set; }
        public int NextMileage { get; set; }

        public int CarId { get; set; }
        public Car? Car { get; set; }
    }
}
