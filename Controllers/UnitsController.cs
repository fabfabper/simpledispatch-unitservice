using Microsoft.AspNetCore.Mvc;
using SimpleDispatch.ServiceBase.Controllers;
using SimpleDispatch.ServiceBase.Interfaces;
using SimpleDispatch.ServiceBase.Database.Interfaces;
using SimpleDispatch.ServiceBase.Models;
using SimpleDispatch.SharedModels.Entities;
using simpledispatch_unitservice.Repositories;

namespace simpledispatch_unitservice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UnitsController : BaseApiController
{
    private readonly IDatabaseRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnitsController> _logger;

    public UnitsController(
        IRabbitMqClient rabbitMqClient,
        IDatabaseRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UnitsController> logger) : base(rabbitMqClient)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Unit>>>> GetAllUnits()
    {
        try
        {
            var units = await _repository.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<Unit>>.CreateSuccess(units, "Units retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all units");
            return StatusCode(500, ApiResponse<IEnumerable<Unit>>.CreateError(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Unit>>> GetUnit(string id)
    {
        try
        {
            var unit = await _repository.GetByIdAsync(id);
            if (unit == null)
            {
                return NotFound(ApiResponse<Unit>.CreateError("Unit not found"));
            }

            return Ok(ApiResponse<Unit>.CreateSuccess(unit, "Unit retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unit {UnitId}", id);
            return StatusCode(500, ApiResponse<Unit>.CreateError(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Unit>>> CreateUnit([FromBody] Unit unit)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var createdUnit = await _repository.AddAsync(unit);
            await _repository.SaveChangesAsync();
            
            // Publish event to RabbitMQ
            await RabbitMqClient.PublishMessageAsync(
                $"{{\"messageType\":\"UnitCreated\",\"unitId\":\"{createdUnit.Id}\"}}");

            await _unitOfWork.CommitTransactionAsync();

            return CreatedAtAction(
                nameof(GetUnit), 
                new { id = createdUnit.Id }, 
                ApiResponse<Unit>.CreateSuccess(createdUnit, "Unit created successfully"));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error creating unit");
            return StatusCode(500, ApiResponse<Unit>.CreateError(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Unit>>> UpdateUnit(string id, [FromBody] Unit unit)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Check if unit exists
            var existingUnit = await _repository.GetByIdAsync(id);
            if (existingUnit == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return NotFound(ApiResponse<Unit>.CreateError("Unit not found"));
            }

            unit.Id = id;
            var updatedUnit = await _repository.UpdateAsync(unit);
            await _repository.SaveChangesAsync();

            // Publish event to RabbitMQ
            await RabbitMqClient.PublishMessageAsync(
                $"{{\"messageType\":\"UnitUpdated\",\"unitId\":\"{updatedUnit.Id}\"}}");

            await _unitOfWork.CommitTransactionAsync();

            return Ok(ApiResponse<Unit>.CreateSuccess(updatedUnit, "Unit updated successfully"));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error updating unit {UnitId}", id);
            return StatusCode(500, ApiResponse<Unit>.CreateError(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteUnit(string id)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Check if unit exists
            var existingUnit = await _repository.GetByIdAsync(id);
            if (existingUnit == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return NotFound(ApiResponse<string>.CreateError("Unit not found"));
            }

            await _repository.DeleteAsync(existingUnit);
            await _repository.SaveChangesAsync();

            // Publish event to RabbitMQ
            await RabbitMqClient.PublishMessageAsync(
                $"{{\"messageType\":\"UnitDeleted\",\"unitId\":\"{id}\"}}");

            await _unitOfWork.CommitTransactionAsync();

            return Ok(ApiResponse<string>.CreateSuccess("Unit deleted", "Unit deleted successfully"));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error deleting unit {UnitId}", id);
            return StatusCode(500, ApiResponse<string>.CreateError(ex.Message));
        }
    }
}
