using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using dotnetapp.Models;
using dotnetapp.Data;

namespace dotnetapp.Services
{
    public class PlantService
    {
        private readonly ApplicationDbContext _context;

        public PlantService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Plant>> GetAllPlants()
        {
            return await _context.Plants.ToListAsync();
        }

        public async Task<Plant> GetPlantById(int plantId)
        {
            var plant = await _context.Plants.FindAsync(plantId);
            if (plant == null)
            {
                throw new KeyNotFoundException($"Plant with ID {plantId} not found.");
            }

            return plant;
        }


        public async Task<bool> AddPlant(Plant plant)
        {
            bool exists = await _context.Plants.AnyAsync(p => p.Name == plant.Name);
            if (exists) return false;

            _context.Plants.Add(plant);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePlant(int plantId, Plant plant)
        {
            var existingPlant = await _context.Plants.FindAsync(plantId);
            if (existingPlant == null) return false;

           

            existingPlant.Name = plant.Name;
            existingPlant.Category = plant.Category;
            existingPlant.Price = plant.Price;

            //correction
            existingPlant.Tips = plant.Tips;
            existingPlant.PlantImage = plant.PlantImage;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePlant(int plantId)
        {
            var plant = await _context.Plants.FindAsync(plantId);
            if (plant == null) return false;

            _context.Plants.Remove(plant);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}