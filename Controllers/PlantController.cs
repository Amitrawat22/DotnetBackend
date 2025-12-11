using Microsoft.AspNetCore.Mvc;
using dotnetapp.Models;
using dotnetapp.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using dotnetapp.Data;
namespace dotnetapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlantController : ControllerBase
    {
        private readonly PlantService _plantService;
        private readonly ApplicationDbContext _context;

        public PlantController(PlantService plantService, ApplicationDbContext context)
        {
            _plantService = plantService;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.Gardener + "," + UserRoles.Customer)]
        public async Task<ActionResult<IEnumerable<Plant>>> GetAllPlants()
        {
            try
            {
                var plants = await _plantService.GetAllPlants();
                return Ok(plants);
            }
            catch (Exception e)
            {

                var log = new ErrorLog
                {
                    Source = "PlantController.GetAllPlants",
                    Message = e.Message,
                    TimeStamp = DateTime.UtcNow
                };

                _context.ErrorLogs.Add(log);
                await _context.SaveChangesAsync();
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("{plantId}")]
        [Authorize(Roles = UserRoles.Gardener)]
        public async Task<ActionResult<Plant>> GetPlantById(int plantId)
        {
            try
            {
                var plant = await _plantService.GetPlantById(plantId);
                return Ok(plant);
            }
            catch (Exception e)
            {

                var log = new ErrorLog
                {
                    Source = "PlantController.GetPlantById",
                    Message = e.Message,
                    TimeStamp = DateTime.UtcNow
                };

                _context.ErrorLogs.Add(log);
                await _context.SaveChangesAsync();
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Gardener)]
        public async Task<ActionResult> AddPlant([FromBody] Plant plant)
        {
            try
            {
                var result = await _plantService.AddPlant(plant);
                if (!result)
                {
                    var log = new ErrorLog
                    {
                        Source = "PlantController.AddPlant",
                        Message = "Bad Request ",
                        TimeStamp = DateTime.UtcNow
                    };

                    _context.ErrorLogs.Add(log);
                    await _context.SaveChangesAsync();
                    return StatusCode(400, "Bad Request");
                }
                return CreatedAtAction(nameof(AddPlant), "Plant added successfully.");
            }
            catch (Exception e)
            {

                var log = new ErrorLog
                {
                    Source = "PlantController.AddPlant",
                    Message = e.Message,
                    TimeStamp = DateTime.UtcNow
                };

                _context.ErrorLogs.Add(log);
                await _context.SaveChangesAsync();
                return StatusCode(500, e.Message);
            }
        }

        [HttpPut("{plantId}")]
        [Authorize(Roles = UserRoles.Gardener)]
        public async Task<ActionResult> UpdatePlant(int plantId, [FromBody] Plant plant)
        {
            try
            {
                var result = await _plantService.UpdatePlant(plantId, plant);
                if (!result)
                {

                    var log = new ErrorLog
                    {
                        Source = "PlantController.UpdatePlant",
                        Message = "Not Found ",
                        TimeStamp = DateTime.UtcNow
                    };

                    _context.ErrorLogs.Add(log);
                    await _context.SaveChangesAsync();
                    return NotFound("Cannot find any plant.");

                }
                return Ok("Plant updated successfully.");
            }
            catch (Exception e)
            {

                var log = new ErrorLog
                {
                    Source = "PlantController.UpdatePlant",
                    Message = e.Message,
                    TimeStamp = DateTime.UtcNow
                };

                _context.ErrorLogs.Add(log);
                await _context.SaveChangesAsync();
                return StatusCode(500, e.Message);
            }
        }

        [HttpDelete("{plantId}")]
        [Authorize(Roles = UserRoles.Gardener)]
        public async Task<IActionResult> DeletePlant(int plantId)
        {
            try
            {
                var result = await _plantService.DeletePlant(plantId);
                if (!result)
                {
                    var log = new ErrorLog
                    {
                        Source = "PlantController.DeletePlant",
                        Message = "Not Found ",
                        TimeStamp = DateTime.UtcNow
                    };

                    _context.ErrorLogs.Add(log);
                    await _context.SaveChangesAsync();
                    return NotFound("Cannot find any plant.");
                }

                return Ok("Plant deleted successfully.");
            }
            catch (Exception e)
            {

                var log = new ErrorLog
                {
                    Source = "PlantController.DeletePlant",
                    Message = e.Message,
                    TimeStamp = DateTime.UtcNow
                };

                _context.ErrorLogs.Add(log);
                await _context.SaveChangesAsync();
                return StatusCode(500, $"Internal Server Error: {e.Message}");
            }
        }
    }
}