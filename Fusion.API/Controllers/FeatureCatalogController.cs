
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.FeatureCatalog;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.FeatureCatalog.Requests;
using Fusion.Service.ViewModels.FeatureCatalog.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeatureCatalogController : ControllerBase
    {
        private readonly IFeatureCatalogService _service;

        public FeatureCatalogController(IFeatureCatalogService service)
        {
            _service = service;
        }

        // POST: api/FeatureCatalog/create
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<FeatureResponse>))]
        public async Task<IActionResult> Create(
            [FromBody] FeatureCreateRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);

            return Ok(ResponseModel<FeatureResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "feature")));
        }

        // PUT: api/FeatureCatalog/{id}
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<FeatureResponse>))]
        public async Task<IActionResult> Update(
            [FromBody] FeatureUpdateRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.UpdateAsync(request, cancellationToken);

            return Ok(ResponseModel<FeatureResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "feature")));
        }

        // GET: api/FeatureCatalog/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<FeatureResponse>))]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);

            return Ok(ResponseModel<FeatureResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "feature")));
        }

        [HttpPatch("{id:guid}/toggle")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<object>))]
        public async Task<IActionResult> Toggle(
          [FromRoute] Guid id,
          [FromQuery] bool active = true,
          CancellationToken cancellationToken = default)
        {
            await _service.ToggleActiveAsync(id, active, cancellationToken);

            return Ok(ResponseModel<object>.Ok(
                data: new { id, active },
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "status of feature")));
        }

        // DELETE: api/FeatureCatalog/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var ok = await _service.DeleteAsync(id, cancellationToken);

            return Ok(ResponseModel<bool>.Ok(
                data: ok,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.DELETE_SUCCESS, "feature")));
        }


        // GET: api/FeatureCatalog/active
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<FeatureActiveResponse>>))]
        public async Task<IActionResult> GetAllActive(CancellationToken cancellationToken)
        {
            var data = await _service.GetAllActiveAsync(cancellationToken);
            return Ok(ResponseModel<List<FeatureActiveResponse>>.Ok(
                data: data,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, " feature active list")));
        }

        // GET: api/FeatureCatalog
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<FeatureResponse>>))]
        public async Task<IActionResult> GetAll(
            [FromQuery] FeatureCatalogPagedRequest request,
            CancellationToken cancellationToken)
        {
            var paged = await _service.GetAllAsync(request, cancellationToken);
            return Ok(ResponseModel<PagedResult<FeatureResponse>>.Ok(
                data: paged,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "feature list")));
        }
    }
}
