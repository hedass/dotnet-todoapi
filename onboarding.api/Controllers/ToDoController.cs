using Microsoft.AspNetCore.Mvc;
using onboarding.bll.DTO;
using onboarding.bll.Interfaces;
using onboarding.bll.Services;
using onboarding.dal.Models;

namespace onboarding.api.Controllers
{
    [ApiController]
    [Route("api/todo")]
    public class ToDoController : ControllerBase
    {
        private readonly IToDoService _toDoService;

        public ToDoController(IToDoService toDoService)
        {
            _toDoService = toDoService;
        }

        /// <summary>
        /// Get All List of ToDos
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Returns the all todo</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAllToDos()
        {
            return Ok(_toDoService.GetAllToDos());
        }

        /// <summary>
        /// Get todo by given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">Returns the coresponding todo</response>
        /// <response code="404">If the todo with given id doesnt exist</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetToDoById(int id)
        {
            var toDoItem = _toDoService.GetToDoById(id);

            if (toDoItem == null)
            {
                return NotFound();
            }

            return Ok(toDoItem);
        }

        /// <summary>
        /// Create and save todo
        /// </summary>
        /// <param name="toDoItemRequest"></param>
        /// <returns></returns>
        /// <response code="201">Returns the newly created todo</response>
        /// <response code="400">If the todo doesnt have a title</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddToDo([FromBody] ToDoItemDTO toDoItemRequest)
        {
            if (string.IsNullOrEmpty(toDoItemRequest.Title))
            {
                return BadRequest("Todo title is required.");
            }

            var addedToDo = _toDoService.AddToDo(toDoItemRequest.Title);

            return CreatedAtAction(nameof(GetToDoById), new { id = addedToDo.Id }, addedToDo);
        }

        /// <summary>
        /// Delete todo by given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">if delete success</response>
        /// <response code="404">If delete failed / item not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteToDoById(int id)
        {

            bool success = _toDoService.DeleteToDoById(id);
            if (!success)
            {
                return NotFound();
            }

            return Ok();
        }

        /// <summary>
        /// Update todo by given id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestBody"></param>
        /// <returns></returns>
        /// <response code="200">if update success</response>
        /// <response code="404">If update failed / item not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateToDo([FromRoute]int id, [FromBody] ToDoItemDTO requestBody)
        {
            ToDoItem? toDoItem = _toDoService.UpdateToDo(id, requestBody.Title, (bool)requestBody.Completed);
            if (toDoItem is null)
            {
                return NotFound();
            }

            return Ok(toDoItem);
        }

        /// <summary>
        /// Search todo by title
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <response code="200">if search success</response>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetToDoByTitle([FromQuery] string query)
        {

            List<ToDoItem> toDoItem = _toDoService.GetToDoByTitle(query);

            return Ok(toDoItem);
        }

    }
}
