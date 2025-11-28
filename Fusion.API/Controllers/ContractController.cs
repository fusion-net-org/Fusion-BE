using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.Services;
using Fusion.Service.ViewModels.Contract.Requests;
using Fusion.Service.ViewModels.Contract.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fusion.API.Controllers
{
    [Route("api/contract")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly IContractService _contractService;

        public ContractController(IContractService contractService)
        {
            _contractService = contractService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ContractResponse>))]
        public async Task<IActionResult> CreateContractAsync([FromBody] CreateContractRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Don't find token!"));

            var response = await _contractService.CreateContractAsync(userId, request);

            return Ok(ResponseModel<ContractResponse>.Ok(
                data: response,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "contract")));
        }


        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ContractResponse>))]
        public async Task<IActionResult> UpdateContractAsync( Guid id, [FromBody] UpdateContractRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Don't find token!"));

            var response = await _contractService.UpdateContractAsync(id, userId, request);

            return Ok(ResponseModel<ContractResponse>.Ok(
                data: response,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "contract")));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<ContractResponse>))]
        public async Task<IActionResult> GetContractById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _contractService.GetContractByIdAsync(id, cancellationToken);
            return Ok(ResponseModel<ContractResponse>.Ok(
                data: result,
                message: "Get contract by id successfully"));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<List<ContractResponse>>))]
        public async Task<IActionResult> GetContracts(CancellationToken cancellationToken)
        {
            var result = await _contractService.GetAllContractsAsync(cancellationToken);
            return Ok(ResponseModel<List<ContractResponse>>.Ok(
                data: result,
                message: "Get contracts successfully"));
        }
        [HttpPost("{id:guid}/upload")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<string>))]
        public async Task<IActionResult> UploadContractAttachment(Guid id, IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ResponseModel<string>.Error(StatusCodes.Status400BadRequest, "File is required."));

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(ResponseModel<string>.Error(StatusCodes.Status401Unauthorized, "Don't find token!"));

            var attachmentUrl = await _contractService.UploadContractAttachmentAsync(id, file, userId, cancellationToken);

            return Ok(ResponseModel<string>.Ok(
                data: attachmentUrl,
                message: "Contract file uploaded successfully"));
        }

    }
}
