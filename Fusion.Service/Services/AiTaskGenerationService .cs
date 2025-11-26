using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.AITaskGenerate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.Service.Services
{
    public interface IAiTaskGenerationService
    {
        Task<AiGenerateTasksResponseDto> GenerateTasksAsync(
            AiTaskGenerateRequestDto request,
            CancellationToken ct = default);

        Task<List<ProjectTask>> SaveGeneratedTasksAsync(
            Guid projectId,
            Guid sprintId,
            AiGenerateTasksResponseDto ai,
            CancellationToken ct = default);

        /// <summary>
        /// Convenience: generate tasks and persist them in DB in one call.
        /// </summary>
        Task<List<ProjectTask>> GenerateAndSaveAsync(
            AiTaskGenerateRequestDto request,
            CancellationToken ct = default);
    }

    public sealed class AiTaskGenerationService : IAiTaskGenerationService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly FusionDbContext _db;
        private readonly AiTaskGenerationOptions _options;
        private readonly ILogger<AiTaskGenerationService> _logger;

        public AiTaskGenerationService(
            IHttpClientFactory httpClientFactory,
            FusionDbContext db,
            IOptions<AiTaskGenerationOptions> options,
            ILogger<AiTaskGenerationService> logger)
        {
            _http = httpClientFactory.CreateClient("openai");
            _db = db;
            _options = options.Value;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // ✅ Converter “hiền”: sprintId lởm → null, không quăng lỗi
            _jsonOptions.Converters.Add(new NullableGuidLenientConverter());
        }

        public async Task<AiGenerateTasksResponseDto> GenerateTasksAsync(
            AiTaskGenerateRequestDto request,
            CancellationToken ct = default)
        {
            var systemPrompt = BuildSystemPrompt();
            var userPayload = BuildUserPayload(request);

            var body = new
            {
                model = _options.Model,
                temperature = _options.Temperature,
                max_tokens = _options.MaxTokens,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPayload }
                },
                response_format = new { type = "json_object" }
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions")
            {
                Content = JsonContent.Create(body, options: _jsonOptions)
            };

            HttpResponseMessage response;
            try
            {
                response = await _http.SendAsync(httpRequest, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling OpenAI chat completions API.");
                throw;
            }

            var json = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "OpenAI API returned non-success status {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    json);
                response.EnsureSuccessStatusCode(); // will throw
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(content))
                    throw new InvalidOperationException("AI returned empty content.");

                var result = JsonSerializer.Deserialize<AiGenerateTasksResponseDto>(
                    content, _jsonOptions);

                if (result == null)
                    throw new InvalidOperationException("Cannot parse AI JSON result.");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse AI JSON response. Raw: {Json}", json);
                throw;
            }
        }

        public async Task<List<ProjectTask>> GenerateAndSaveAsync(
            AiTaskGenerateRequestDto request,
            CancellationToken ct = default)
        {
            // ===== Lấy danh sách sprint trên board (nếu có) =====
            var boardSprints = (request.BoardSprints ?? new List<AiBoardSprintDto>())
                .Where(s => s.Id != Guid.Empty)
                .ToList();

            // Không có board hoặc chỉ 1 sprint → hành vi cũ: generate cho sprint hiện tại
            if (boardSprints.Count <= 1)
            {
                var aiSingle = await GenerateTasksAsync(request, ct);

                // Đảm bảo tất cả task có SprintId hợp lệ
                foreach (var t in aiSingle.Tasks)
                {
                    if (!t.SprintId.HasValue || t.SprintId.Value == Guid.Empty)
                        t.SprintId = request.SprintId;
                }

                return await SaveGeneratedTasksAsync(
                    request.ProjectId,
                    request.SprintId,
                    aiSingle,
                    ct);
            }

            // ===== Multi-sprint mode: quantity = số task / sprint =====
            // Gọi AI cho từng sprint, mỗi sprint ~ request.Quantity task
            var merged = new AiGenerateTasksResponseDto
            {
                Tasks = new List<AiGeneratedTaskDraftDto>()
            };

            foreach (var sp in boardSprints)
            {
                // Ghi đè context sprint cho lần gọi này
                request.SprintId = sp.Id;
                request.SprintName = sp.Name;
                request.SprintStart = sp.Start;
                request.SprintEnd = sp.End;
                request.SprintCapacityHours = sp.CapacityHours;

                var aiForSprint = await GenerateTasksAsync(request, ct);

                if (aiForSprint?.Tasks == null || aiForSprint.Tasks.Count == 0)
                    continue;

                foreach (var t in aiForSprint.Tasks)
                {
                    // Nếu AI không gán sprintId hoặc gán bậy → ép về sprint hiện tại
                    if (!t.SprintId.HasValue || t.SprintId.Value == Guid.Empty)
                        t.SprintId = sp.Id;

                    merged.Tasks.Add(t);
                }
            }

            // Sau khi merge: tất cả task trong merged.Tasks đều đã có SprintId hợp lệ
            return await SaveGeneratedTasksAsync(
                request.ProjectId,
                request.SprintId,
                merged,
                ct);
        }

        private string BuildSystemPrompt()
        {
            const string defaultPrompt = """
You are an assistant that helps a product team break down work into sprint tasks
for an Agile project management tool called FUSION.

Rules:
- Always respond with valid JSON that matches the schema given in the user message.
- Do NOT assign tasks to specific people. Do not output assignee names, ids or emails.
- Use the existing tasks list as context: avoid duplicates and instead generate tasks that cover missing flows, edge cases and testability.
- Prefer short, action-oriented titles.
- If dependencies are requested, only refer to existing tasks by code or title that appear in the context.
- When checklists are requested, generate 3–7 clear, testable checklist items per task.
- If estimate is requested, keep each task size consistent with the sprint capacity and the given range.
""";

            return defaultPrompt;
        }

        private string BuildUserPayload(AiTaskGenerateRequestDto r)
        {
            var obj = new
            {
                instructions = new
                {
                    goal = r.Goal,
                    context = r.Context,
                    sprint = new
                    {
                        id = r.SprintId,
                        name = r.SprintName,
                        start = r.SprintStart,
                        end = r.SprintEnd,
                        capacityHours = r.SprintCapacityHours
                    },
                    workflow = new
                    {
                        statuses = r.WorkflowStatuses,
                        defaultStatusId = r.DefaultStatusId
                    },
                    // toàn bộ board (nếu có)
                    board = (r.BoardSprints != null || r.BoardTasks != null)
                        ? new
                        {
                            sprints = r.BoardSprints,
                            tasks = r.BoardTasks
                        }
                        : null,
                    workTypes = r.WorkTypes,
                    modules = r.Modules,
                    quantity = r.Quantity,
                    granularity = r.Granularity,
                    estimate = new
                    {
                        unit = r.EstimateUnit,
                        withEstimate = r.WithEstimate,
                        min = r.EstimateMin,
                        max = r.EstimateMax,
                        totalEffortHours = r.TotalEffortHours
                    },
                    deadline = r.Deadline,
                    teamContext = new
                    {
                        memberCount = r.TeamMemberCount,
                        roles = r.TeamRoles,
                        techStack = r.TechStack
                    },
                    requirements = new
                    {
                        functional = r.FunctionalRequirements,
                        nonFunctional = r.NonFunctionalRequirements,
                        acceptanceHint = r.AcceptanceHint
                    },
                    duplicateStrategy = new
                    {
                        includeExistingTasks = r.IncludeExistingTasks,
                        avoidSameTitle = r.AvoidSameTitle,
                        avoidSameDescription = r.AvoidSameDescription
                    },
                    existingTasks = r.IncludeExistingTasks
                        ? r.ExistingTasksSnapshot
                        : null
                },
                outputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        tasks = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "object",
                                required = new[] { "title", "type", "priority" },
                                properties = new
                                {
                                    title = new { type = "string" },
                                    description = new { type = "string" },
                                    type = new { type = "string" },
                                    priority = new { type = "string" },
                                    severity = new { type = "string" },

                                    // sprint: AI có thể điền hoặc bỏ trống
                                    sprintId = new
                                    {
                                        type = "string",
                                        description = "Id of a sprint from instructions.board.sprints[].id. If unsure, set null or omit."
                                    },
                                    sprintName = new
                                    {
                                        type = "string",
                                        description = "Optional sprint name for readability"
                                    },

                                    statusCategory = new { type = "string" },
                                    statusCode = new { type = "string" },
                                    estimateHours = new { type = "integer" },
                                    storyPoints = new { type = "integer" },
                                    dueDate = new { type = "string" },
                                    module = new { type = "string" },
                                    acceptanceCriteria = new { type = "string" },
                                    checklist = new
                                    {
                                        type = "array",
                                        items = new { type = "string" }
                                    },
                                    dependsOnCodes = new
                                    {
                                        type = "array",
                                        items = new { type = "string" }
                                    },
                                    dependsOnTitles = new
                                    {
                                        type = "array",
                                        items = new { type = "string" }
                                    }
                                }
                            }
                        }
                    },
                    required = new[] { "tasks" }
                },
                instructionsForModel =
                    $"You are generating tasks for ONE sprint at a time (instructions.sprint). " +
                    $"For each call, try to generate around {r.Quantity} sprint-ready tasks for that sprint. " +
                    "Use instructions.board.sprints and instructions.board.tasks as global context. " +
                    "If you are not sure which sprint a task belongs to, omit sprintId or set it to null; do not invent random IDs."
            };

            return JsonSerializer.Serialize(obj, _jsonOptions);
        }

        public async Task<List<ProjectTask>> SaveGeneratedTasksAsync(
            Guid projectId,
            Guid sprintId,
            AiGenerateTasksResponseDto ai,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var tasks = new List<ProjectTask>();

            // 1) Project + Workflow
            var project = await _db.Projects.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId, ct)
                ?? throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Project"));

            var workflowId = project.WorkflowId;
            if (workflowId == null || workflowId == Guid.Empty)
                throw CustomExceptionFactory.CreateBadRequestError(
                    "Workflow is not configured for this project.");

            var statuses = await _db.WorkflowStatuses.AsNoTracking()
                .Where(x => x.WorkflowId == workflowId)
                .OrderBy(x => x.Position)
                .ToListAsync(ct);

            if (statuses.Count == 0)
                throw CustomExceptionFactory.CreateBadRequestError("Workflow has no statuses.");

            var defaultStatus =
                statuses.FirstOrDefault(x => x.IsStart) ??
                statuses.FirstOrDefault(x => x.Category == "TODO") ??
                statuses.First();

            // 2) Code sequence chung cho cả project
            var existingCount = await _db.ProjectTasks.AsNoTracking()
                .LongCountAsync(t => t.ProjectId == projectId, ct);

            var seq = existingCount;
            var codePrefix = string.IsNullOrWhiteSpace(project.Code) ? "PRJ" : project.Code!;

            // 3) OrderInSprint: quản lý theo từng sprint
            var orderBySprint = await _db.ProjectTasks.AsNoTracking()
                .Where(t => t.ProjectId == projectId
                            && t.CurrentStatusId == defaultStatus.Id
                            && !t.IsDeleted)
                .GroupBy(t => t.SprintId)
                .Select(g => new { SprintId = g.Key, MaxOrder = g.Max(t => t.OrderInSprint) })
                .ToDictionaryAsync(
                    x => x.SprintId ?? sprintId,
                    x => x.MaxOrder,
                    ct);

            foreach (var g in ai.Tasks)
            {
                seq++;

                // Sprint mục tiêu: ưu tiên SprintId đã set, fallback về sprintId route
                var targetSprintId = g.SprintId.HasValue && g.SprintId.Value != Guid.Empty
                    ? g.SprintId.Value
                    : sprintId;

                if (!orderBySprint.TryGetValue(targetSprintId, out var currentOrder))
                {
                    currentOrder = 0;
                }
                currentOrder++;
                orderBySprint[targetSprintId] = currentOrder;

                var statusId = ResolveStatusId(g, statuses) ?? defaultStatus.Id;
                var status = statuses.First(s => s.Id == statusId);

                var priority = NormalizePriority(g.Priority);
                var severity = NormalizeSeverity(g.Severity);
                var dueDate = g.DueDate;

                var entity = new ProjectTask
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    SprintId = targetSprintId,
                    Type = g.Type,
                    Title = g.Title,
                    Description = g.Description,
                    Priority = priority,
                    Severity = severity,
                    IsBacklog = false,
                    Point = g.StoryPoints,
                    Status = !string.IsNullOrWhiteSpace(status.Code)
                        ? status.Code
                        : status.Name,
                    DueDate = dueDate,
                    Source = "AI",
                    WithdrawnAt = null,
                    CreatedBy = null,
                    CreateAt = now,
                    UpdateAt = now,
                    OrderInSprint = currentOrder,
                    IsDeleted = false,
                    Code = $"{codePrefix}-T-{seq:000}",
                    EstimateHours = g.EstimateHours,
                    RemainingHours = g.EstimateHours,
                    CurrentStatusId = status.Id,
                    ParentTaskId = null,
                    CarryOverCount = 0,
                    SourceTaskId = null
                };

                if (g.Checklist != null && g.Checklist.Count > 0)
                {
                    int checklistOrder = 0;
                    foreach (var label in g.Checklist.Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        entity.ChecklistItems.Add(new ProjectTaskChecklistItem
                        {
                            Id = Guid.NewGuid(),
                            TaskId = entity.Id,
                            Label = label.Trim(),
                            IsDone = false,
                            OrderIndex = checklistOrder++,
                            CreatedAt = now
                        });
                    }
                }

                tasks.Add(entity);
                _db.ProjectTasks.Add(entity);
            }

            await _db.SaveChangesAsync(ct);

            return tasks;
        }

        private Guid? ResolveStatusId(
            AiGeneratedTaskDraftDto g,
            List<WorkflowStatus> statuses)
        {
            // 1. Match theo StatusCode
            if (!string.IsNullOrWhiteSpace(g.StatusCode))
            {
                var match = statuses.FirstOrDefault(st =>
                    !string.IsNullOrWhiteSpace(st.Code) &&
                    st.Code!.Equals(g.StatusCode, StringComparison.OrdinalIgnoreCase));

                if (match != null)
                    return match.Id;
            }

            // 2. Match theo StatusCategory (TODO/IN_PROGRESS/REVIEW/DONE...)
            if (!string.IsNullOrWhiteSpace(g.StatusCategory))
            {
                var cat = g.StatusCategory.Trim().ToUpperInvariant();
                var match = statuses.FirstOrDefault(st =>
                    !string.IsNullOrWhiteSpace(st.Category) &&
                    st.Category!.ToUpper() == cat);

                if (match != null)
                    return match.Id;
            }

            // 3. Không match thì trả null để caller dùng defaultStatus (IsStart)
            return null;
        }

        private static string NormalizePriority(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Medium";
            return raw.Trim().ToLowerInvariant() switch
            {
                "urgent" => "Urgent",
                "high" => "High",
                "medium" => "Medium",
                "low" => "Low",
                _ => "Medium"
            };
        }

        private static string? NormalizeSeverity(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            return raw.Trim().ToLowerInvariant() switch
            {
                "critical" => "Critical",
                "high" => "High",
                "medium" => "Medium",
                "low" => "Low",
                _ => null
            };
        }
    }

    /// <summary>
    /// Converter “hiền” cho Guid?: string invalid → null, không quăng exception.
    /// Dùng để đọc sprintId từ AI.
    /// </summary>
    internal sealed class NullableGuidLenientConverter : JsonConverter<Guid?>
    {
        public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrWhiteSpace(s))
                    return null;

                return Guid.TryParse(s, out var g) ? g : (Guid?)null;
            }

            // Bất kỳ format khác → bỏ qua, trả null
            return null;
        }

        public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }
}
