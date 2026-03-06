using SoftLedger.Models;

namespace SoftLedger.Repositories
{
    public class SoftwareRepository
    {
        private static readonly List<Software> Softwares = new();

        public static IEnumerable<Software> GetAll()
            => Softwares;

        public static void AddRange(IEnumerable<Software> softwares)
            => Softwares.AddRange(softwares);

        public static bool Delete(Guid id)
        {
            var software = Softwares.FirstOrDefault(s => s.Id == id);
            if (software == null) return false;

            Softwares.Remove(software);
            return true;
        }
    }
}

