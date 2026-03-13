using Microsoft.EntityFrameworkCore;
using SmartRecycle.Models;

namespace SmartRecycle.Repositories
{
    public class DetectionRepository : IDetectionRepository
    {
        private readonly SmartRecycleContext _context;

        public DetectionRepository(SmartRecycleContext context)
        {
            _context = context;
        }

        // إضافة اكتشاف جديد
        public async Task<Detection> AddDetectionAsync(Detection detection)
        {
            _context.Detections.Add(detection);
            await _context.SaveChangesAsync();
            return detection;
        }

        // جلب كل الاكتشافات (الأحدث أولاً)
        public async Task<IEnumerable<Detection>> GetAllDetectionsAsync(int limit = 100)
        {
            return await _context.Detections
                //.Include(d => d.Machine)
                .OrderByDescending(d => d.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        // جلب اكتشاف واحد بالـ ID
        public async Task<Detection?> GetDetectionByIdAsync(int id)
        {
            return await _context.Detections
                //.Include(d => d.Machine)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        // حذف جميع الاكتشافات
        public async Task<bool> DeleteAllDetectionsAsync()
        {
            var allDetections = await _context.Detections.ToListAsync();
            _context.Detections.RemoveRange(allDetections);
            await _context.SaveChangesAsync();
            return true;
        }

        // الحصول على الإحصائيات
        public async Task<DetectionStatistics> GetStatisticsAsync()
        {
            var stats = await _context.Detections
                .GroupBy(d => 1)
                .Select(g => new DetectionStatistics
                {
                    TotalScans = g.Count(),
                    TotalGlassBottles = g.Sum(d => d.GlassBottles),
                    TotalPlasticBottles = g.Sum(d => d.PlasticBottles),
                    TotalCans = g.Sum(d => d.Cans),
                    TotalItems = g.Sum(d => d.TotalItems)
                })
                .FirstOrDefaultAsync();

            return stats ?? new DetectionStatistics();
        }

        // جلب الاكتشافات حسب فترة زمنية
        public async Task<IEnumerable<Detection>> GetDetectionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Detections
                //.Include(d => d.Machine)
                .Where(d => d.Timestamp >= startDate && d.Timestamp <= endDate)
                .OrderByDescending(d => d.Timestamp)
                .ToListAsync();
        }

        // جلب الاكتشافات حسب ماكينة معينة
        public async Task<IEnumerable<Detection>> GetDetectionsByMachineIdAsync(int machineId)
        {
            return await _context.Detections
                //.Include(d => d.Machine)
                //.Where(d => d.MachineId == machineId)
                .OrderByDescending(d => d.Timestamp)
                .ToListAsync();
        }
    }
}