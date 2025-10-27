using System.Security.Claims;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Comment.Request;
using Fusion.Service.ViewModels.Comment.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/comments")]
    [ApiController]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        /// <summary>
        /// Create a new comment
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CommentResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateComment([FromBody] CommentRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            var result = await _commentService.CreateCommentAsync(request, userId);

            return Ok(ResponseModel<CommentResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "comment")));
        }

        /// <summary>
        /// Get all comments
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<CommentResponse>>))]
        public async Task<IActionResult> GetAllComments()
        {
            var result = await _commentService.GetAllCommentsAsync();
            return Ok(ResponseModel<IEnumerable<CommentResponse>>.Ok(result));
        }

        /// <summary>
        /// Get comment by id
        /// </summary>
        [HttpGet("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CommentResponse>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCommentById(long id)
        {
            var result = await _commentService.GetCommentByIdAsync(id);
            if (result == null)
            {
                return NotFound(ResponseModel<string>.Error(
                    StatusCodes.Status404NotFound,
                    ResponseMessageHelper.FormatMessage(ResponseMessages.NOT_FOUND, "comment")));
            }

            return Ok(ResponseModel<CommentResponse>.Ok(result));
        }

        /// <summary>
        /// Delete a comment by id
        /// </summary>
        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteComment(long id)
        {
            var deleted = await _commentService.DeleteCommentAsync(id);

            if (!deleted)
            {
                return NotFound(ResponseModel<string>.Error(
                    StatusCodes.Status404NotFound,
                    ResponseMessageHelper.FormatMessage(ResponseMessages.NOT_FOUND, "comment")));
            }

            return Ok(ResponseModel<bool>.Ok(true,
                ResponseMessageHelper.FormatMessage(ResponseMessages.DELETE_SUCCESS, "comment")));
        }

        /// <summary>
        /// Update a comment
        /// </summary>
        [HttpPut("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CommentResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateComment(long id, [FromBody] CommentRequestUpdate request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ResponseModel<string>.Error(
                    StatusCodes.Status401Unauthorized,
                    "Don't find token!"));
            }

            request.Id = id;

            var updated = await _commentService.UpdateCommentAsync(request, userId);

            if (updated == null)
            {
                return NotFound(ResponseModel<string>.Error(
                    StatusCodes.Status404NotFound,
                    ResponseMessageHelper.FormatMessage(ResponseMessages.NOT_FOUND, "comment")));
            }

            return Ok(ResponseModel<CommentResponse>.Ok(
                data: updated,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.UPDATE_SUCCESS, "comment")));
        }

    }
}
