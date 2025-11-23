using AutoMapper;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Services;
using Fusion.Service.ViewModels.AITaskGenerate;
using Fusion.Service.ViewModels.Task.Response;
using MailKit;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/sprints/{sprintId:guid}/ai")]
    public class SprintAiController : ControllerBase
    {
        private readonly IAiTaskGenerationService _service;
        private readonly IMapper _mapper;

        public SprintAiController(
            IAiTaskGenerationService service,
            IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Gọi AI sinh danh sách tsk (preview, chưa lưu DB).
        /// </summary>
        [HttpPost("generate-tasks")]
        public async Task<ActionResult<AiGenerateTasksResponseDto>> GenerateTasks(
            Guid projectId,
            Guid sprintId,
            [FromBody] AiTaskGenerateRequestDto request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (projectId != request.ProjectId || sprintId != request.SprintId)
                return BadRequest("ProjectId/SprintId mismatch.");

            var result = await _service.GenerateTasksAsync(request, ct);
            return Ok(result);
            // Nếu bạn có wrapper chuẩn: return Ok(ApiSuccessResponse.Success(result));
        }

        /// <summary>
        /// Gọi AI sinh task và lưu thẳng vào database, trả về TaskResponse để add vào board.
        /// </summary>
        [HttpPost("generate-and-save")]
        public async Task<ActionResult<List<ProjectTaskResponse>>> GenerateAndSave(
            Guid projectId,
            Guid sprintId,
            [FromBody] AiTaskGenerateRequestDto request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (projectId != request.ProjectId || sprintId != request.SprintId)
                return BadRequest("ProjectId/SprintId mismatch.");

            var entities = await _service.GenerateAndSaveAsync(request, ct);
            var vms = _mapper.Map<List<ProjectTaskResponse>>(entities);

            return Ok(vms);
        }
        [HttpPost("tasks/preview")]
        public async Task<ActionResult<AiGenerateTasksResponseDto>> PreviewTasks(
            Guid projectId,
            Guid sprintId,
            [FromBody] AiTaskGenerateRequestDto dto,
            CancellationToken ct)
        {
            if (dto == null)
                throw CustomExceptionFactory.CreateBadRequestError("Invalid request.");

            // Đồng bộ ProjectId & SprintId giữa route và body
            if (dto.ProjectId == Guid.Empty)
                dto.ProjectId = projectId;
            else if (dto.ProjectId != projectId)
                throw CustomExceptionFactory.CreateBadRequestError("ProjectId in body and route must match.");

            if (dto.SprintId == Guid.Empty)
                dto.SprintId = sprintId;
            else if (dto.SprintId != sprintId)
                throw CustomExceptionFactory.CreateBadRequestError("SprintId in body and route must match.");

            var result = await _service.GenerateTasksAsync(dto, ct);
            return Ok(result);
        }

    }
}
