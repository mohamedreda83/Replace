using SmartRecycle.Models;

namespace SmartRecycle.Repositories
{
    public interface IDetectionRepository
    {
        Task<Detection> AddDetectionAsync(Detection detection);
        Task<IEnumerable<Detection>> GetAllDetectionsAsync(int limit = 100);
        Task<Detection?> GetDetectionByIdAsync(int id);
        Task<bool> DeleteAllDetectionsAsync();
        Task<DetectionStatistics> GetStatisticsAsync();
        Task<IEnumerable<Detection>> GetDetectionsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Detection>> GetDetectionsByMachineIdAsync(int machineId);
    }

    public class DetectionStatistics
    {
        public int TotalScans { get; set; }
        public int TotalGlassBottles { get; set; }
        public int TotalPlasticBottles { get; set; }
        public int TotalCans { get; set; }
        public int TotalItems { get; set; }
    }
}