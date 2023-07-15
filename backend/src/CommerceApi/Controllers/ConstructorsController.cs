using CommerceApi.BusinessLayer.Services.Interfaces;
using CommerceApi.Shared.Models;
using CommerceApi.Shared.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

public class ConstructorsController : ControllerBase
{
    private readonly IConstructorService _constructorService;

    public ConstructorsController(IConstructorService constructorService)
    {
        _constructorService = constructorService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Constructor), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] SaveConstructorRequest constructor)
    {
        var saveConstructorResult = await _constructorService.CreateAsync(constructor);
        return CreateResponse(saveConstructorResult, StatusCodes.Status201Created);
    }

    [HttpDelete("{constructorId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid constructorId)
    {
        var deleteConstructorResult = await _constructorService.DeleteAsync(constructorId);
        return CreateResponse(deleteConstructorResult, StatusCodes.Status200OK);
    }

    [HttpGet("{constructorId:guid}")]
    [ProducesResponseType(typeof(Constructor), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid constructorId)
    {
        var constructor = await _constructorService.GetAsync(constructorId);
        return CreateResponse(constructor, StatusCodes.Status200OK);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Constructor>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(string city = null)
    {
        var constructors = await _constructorService.GetListAsync(city);
        return Ok(constructors);
    }

    [HttpPut("{constructorId:guid}")]
    [ProducesResponseType(typeof(Constructor), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid constructorId, [FromBody] SaveConstructorRequest constructor)
    {
        var updateConstructorResult = await _constructorService.UpdateAsync(constructorId, constructor);
        return CreateResponse(updateConstructorResult, StatusCodes.Status200OK);
    }
}