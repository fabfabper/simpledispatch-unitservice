using Microsoft.AspNetCore.Mvc;
using SimpleDispatch.ServiceBase.Controllers;
using SimpleDispatch.ServiceBase.Interfaces;
using SimpleDispatch.ServiceBase.Database.Interfaces;
using SimpleDispatch.SharedModels.Converters;
using simpledispatch_unitservice.Repositories;

using UnitDto = SimpleDispatch.SharedModels.Dtos.Unit;

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
    public async Task<ActionResult<SimpleDispatch.SharedModels.Dtos.ApiResponse<IEnumerable<UnitDto>>>> GetAllUnits()
    {
        try
        {
            var units = await _repository.GetAllAsync();
            var dtos = units.Select(UnitEntityDtoConverters.ToDto);
            return Ok(SimpleDispatch.ServiceBase.Models.ApiResponse<IEnumerable<UnitDto>>.CreateSuccess(dtos, "Units retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all units");
            return StatusCode(500, SimpleDispatch.SharedModels.Dtos.ApiResponse<IEnumerable<UnitDto>>.CreateError(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SimpleDispatch.SharedModels.Dtos.ApiResponse<UnitDto>>> GetUnit(string id)
    {
        try
        {
            var unit = await _repository.GetByIdAsync(id);
            if (unit == null)
                return NotFound(SimpleDispatch.SharedModels.Dtos.ApiResponse<UnitDto>.CreateError("Unit not found"));

            return Ok(SimpleDispatch.SharedModels.Dtos.ApiResponse<UnitDto>.CreateSuccess(UnitEntityDtoConverters.ToDto(unit), "Unit retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unit {UnitId}", id);
            return StatusCode(500, SimpleDispatch.SharedModels.Dtos.ApiResponse<UnitDto>.CreateError(ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<SimpleDispatch.SharedModels.Dtos.ApiResponse<UnitDto>>> CreateUnit([FromBody] UnitDto unit)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var createdUnit = await _repository.AddAsync(UnitEntityDtoConverters.ToEntity(unit));
            await _repository.SaveChangesAsync();
            await RabbitMqClient.PublishMessageAsync(
                $"{{\"messageType\":\"UnitCreated\",\"unitId\":\"{createdUnit.Id}\"}}");
            await _unitOfWork.CommitTransactionAsync();

            return CreatedAtAction(
                nameof(GetUnit),
                new { id = createdUnit.Id },
                SimpleDispatch.SharedModels.Dtos.ApiResponse<UnitDto>.CreateSuccess(UnitEntityDtoConverters.ToDto(createdUnit), "Unit created successfully"));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error creating unit");
            return StatusCode(500, SimpleDispatch.SharedModels.Dtos.ApiResponse<UnitDto>.CreateError(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SimpleDispatch.SharedModels.Dtos.ApiResponse<UnitDto>>> UpdateUnit(string id, [FromBody] UnitDto unit)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            var existingUnit = await _repository.GetByIdAsync(id);
            if (existingUnit == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return NotFound(SimpleDispatch.SharedModels.Dtos.ApiResponse<UnitDto>.CreateError("Unit not found"));
            }

            unit.Id = id;
            var updatedUnit = await _repository.UpdateAsync(UnitEntityDtoConverters.ToEntity(unit));
            await _repository.SaveChangesAsync();
            await RabbitMqClient.PublishMessageAsync(
                $"{{\"messageType\":\"UnitUpdated\",\"unitId\":\"{updatedUnit.Id}\"}}");
            await _unitOfWork.CommitTransactionAsync();

            return Ok(SimpleDispatch.SharedModels.Dtos.ApiResponse<UnitDto>.CreateSuccess(UnitEntityDtoConverters.ToDto(updatedUnit), "Unit updated successfully"));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error updating unit {UnitId}", id);
            return StatusCode(500, SimpleDispatch.SharedModels.Dtos.ApiResponse<UnitDto>.CreateError(ex.Message));
        }
    }

    // ...existing code for DeleteUnit..
}