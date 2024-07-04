using FluentAssertions;
using Moq;
using onboarding.bll.Interfaces;
using onboarding.bll.Services;
using onboarding.bll.test.MockData;
using onboarding.dal.Interface;
using onboarding.dal.Interfaces;
using onboarding.dal.Models;

namespace onboarding.bll.test.Common
{
    public class ToDoServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IToDoRepository> _mockToDoRepository;
        private readonly Mock<IRedisService> _mockRedisService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly ToDoService _toDoService;

        public ToDoServiceTest()
        {

            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockRedisService = new Mock<IRedisService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockToDoRepository = new Mock<IToDoRepository>();

            _mockUnitOfWork.Setup(uow => uow.ToDoRepository)
                           .Returns(_mockToDoRepository.Object);

            _mockToDoRepository.Setup(repo => repo.GetAllToDos())
                              .Returns(ToDoItemMockData.ToDoItems);

            _mockToDoRepository.Setup(repo => repo.GetToDoById(It.IsAny<int>()))
                              .Returns((int id) => ToDoItemMockData.ToDoItems.Find(item => item.Id == id)).Verifiable();

            _mockToDoRepository.Setup(repo => repo.AddToDo(It.IsAny<string>()))
                              .Returns((string title) =>
                              {
                                  var newId = ToDoItemMockData.ToDoItems.Count + 1;
                                  var newToDoItem = new ToDoItem { Id = newId, Title = title };
                                  ToDoItemMockData.ToDoItems.Add(newToDoItem);
                                  return newToDoItem;
                              });

            _mockToDoRepository.Setup(repo => repo.UpdateToDo(It.IsAny<ToDoItem>()))
                              .Callback((ToDoItem toDo) =>
                              {
                                  var existingToDo = ToDoItemMockData.ToDoItems.Find(item => item.Id == toDo.Id);
                                  if (existingToDo != null)
                                  {
                                      existingToDo.Title = toDo.Title;
                                      existingToDo.Completed = toDo.Completed;
                                  }
                              });

            _mockToDoRepository.Setup(repo => repo.DeleteToDo(It.IsAny<ToDoItem>()))
                              .Callback((ToDoItem toDo) =>
                              {
                                  var existingToDo = ToDoItemMockData.ToDoItems.Find(item => item.Id == toDo.Id);
                                  if (existingToDo != null)
                                  {
                                      ToDoItemMockData.ToDoItems.Remove(existingToDo);
                                  }
                              });

            _mockToDoRepository.Setup(repo => repo.GetToDoByTitle(It.IsAny<string>()))
                              .Returns((string title) =>
                              {
                                  return ToDoItemMockData.ToDoItems.FindAll(item => item.Title.ToLower().Contains(title.ToLower()));
                              });


            _mockRedisService.Setup(redis => redis.GetString<ToDoItem>(It.IsAny<string>()))
                             .Returns((ToDoItem)null);

            _mockRedisService.Setup(redis => redis.SetStringAsync<ToDoItem>(It.IsAny<string>(), It.IsAny<ToDoItem>()));

            _toDoService = new ToDoService(
                _mockUnitOfWork.Object,
                _mockRedisService.Object, 
                _mockNotificationService.Object);
        }

        // Reset mock data before each test
        [Fact]
        public void ResetMockData()
        {
            ToDoItemMockData.ToDoItems = ToDoItemMockData.CloneInitialData();
        }

        [Fact]
        public void GetAllToDos_ShouldReturnListOfToDoItems()
        {
            // Arrange
            ResetMockData();

            // Act
            var result = _toDoService.GetAllToDos();

            // Assert
            result.Should().BeEquivalentTo(ToDoItemMockData.ToDoItems);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void GetToDoById_InRedis_Success(int id)
        {
            // Arrange
            ResetMockData();
            var expectedResult = ToDoItemMockData.ToDoItems.Find(t => t.Id == id);
            _mockRedisService.Setup(redis => redis.GetString<ToDoItem>(It.IsAny<string>()))
                             .Returns(expectedResult);

            // Act
            var result = _toDoService.GetToDoById(id);

            // Assert
            result.Should().BeEquivalentTo(ToDoItemMockData.ToDoItems.Find(t => t.Id == id));
            _mockRedisService.Verify(uow => uow.GetString<ToDoItem>($"todoitem:{id}"), Times.Once);
            _mockToDoRepository.Verify(repo => repo.GetToDoById(id), Times.Never);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void GetToDoById_InDb_Success(int id)
        {
            // Arrange
            ResetMockData();
            var expectedResult = ToDoItemMockData.ToDoItems.Find(t => t.Id == id);

            // Act
            var result = _toDoService.GetToDoById(id);

            // Assert
            result.Should().BeEquivalentTo(ToDoItemMockData.ToDoItems.Find(t => t.Id == id));
            _mockRedisService.Verify(uow => uow.GetString<ToDoItem>($"todoitem:{id}"), Times.Once);
            _mockToDoRepository.Verify(repo => repo.GetToDoById(id), Times.Once);
        }

        [Theory]
        [InlineData(9999)]
        public void GetToDoById_NotFound(int id)
        {
            // Arrange
            ResetMockData();
            var expectedResult = ToDoItemMockData.ToDoItems.Find(t => t.Id == id);

            // Act
            var result = _toDoService.GetToDoById(id);

            // Assert
            result.Should().BeNull();
            _mockRedisService.Verify(uow => uow.GetString<ToDoItem>($"todoitem:{id}"), Times.Once);
            _mockToDoRepository.Verify(repo => repo.GetToDoById(id), Times.Once);
        }

        [Theory]
        [InlineData("New Task")]
        public void AddToDo_ShouldReturnNewToDoItem(string title)
        {
            // Arrange
            ResetMockData(); 
            var expectedResult = new ToDoItem { Id = ToDoItemMockData.ToDoItems.Count + 1, Title = title };

            // Act
            var result = _toDoService.AddToDo(title);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            _mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
            _mockRedisService.Verify(uow => uow.SetStringAsync<ToDoItem>($"todoitem:{result.Id}", result), Times.Once);
            _mockNotificationService.Verify(ns => ns.SendNotification($"ToDo item added: {result.Title}"), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        public void DeleteToDoById_Success(int id)
        {
            // Arrange
            ResetMockData();
            var expectedToDoBeingDeleted = ToDoItemMockData.ToDoItems.Find(t => t.Id == id);

            // Act
            var result = _toDoService.DeleteToDoById(id);

            // Assert
            result.Should().BeTrue();
            _mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
            _mockRedisService.Verify(uow => uow.RemoveAsync($"todoitem:{id}"), Times.Once);
            _mockNotificationService.Verify(ns => ns.SendNotification($"ToDo item deleted: {expectedToDoBeingDeleted.Title}"), Times.Once);
        }

        [Theory]
        [InlineData(999)]
        public void DeleteToDoById_NotFound(int id)
        {
            // Arrange
            ResetMockData();

            // Act
            var result = _toDoService.DeleteToDoById(id);

            // Assert
            result.Should().BeFalse();
            _mockUnitOfWork.Verify(uow => uow.Save(), Times.Never);
            _mockRedisService.Verify(uow => uow.RemoveAsync($"todoitem:{id}"), Times.Never);
            _mockNotificationService.Verify(ns => ns.SendNotification($"ToDo item deleted: {string.Empty}"), Times.Never);
        }

        [Theory]
        [InlineData(1, "New Title", true)]
        public void UpdateToDo_Success(int id, string title, bool completed)
        {
            // Arrange
            ResetMockData();
            var oldToDo = ToDoItemMockData.ToDoItems.Find(t => t.Id == id);
            var oldTitle = oldToDo.Title;
            var oldCompleted = oldToDo.Completed;
            var expectedResult = new ToDoItem() { Id = id, Title = title, Completed = completed };

            // Act
            var result = _toDoService.UpdateToDo(id, title, completed);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            _mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
            _mockRedisService.Verify(uow => uow.SetStringAsync($"todoitem:{id}", result), Times.Once);
            _mockNotificationService.Verify(ns => ns.SendNotification($"ToDo item updated: \n{$"{oldTitle} -> {title}\n"}{$"{oldCompleted} -> {completed}\n"}"), Times.Once);
        }

        [Theory]
        [InlineData(999, "New Title", true)]
        public void UpdateToDo_NotFound(int id, string title, bool completed)
        {
            // Arrange
            ResetMockData();

            // Act
            var result = _toDoService.UpdateToDo(id, title, completed);

            // Assert
            result.Should().BeNull();
            _mockUnitOfWork.Verify(uow => uow.Save(), Times.Never);
            _mockRedisService.Verify(uow => uow.SetStringAsync($"todoitem:{id}", result), Times.Never);
            _mockNotificationService.Verify(ns => ns.SendNotification($"ToDo item updated: "), Times.Never);
        }

        [Theory]
        [InlineData("Task")]
        public void GetToDoByTitle_Found(string query)
        {
            // Arrange
            ResetMockData();
            var expectedResults = ToDoItemMockData.ToDoItems.Where(t => t.Title.ToLower().Contains(query.ToLower())).OrderBy(t => t.Title).ToList();

            // Act
            var result = _toDoService.GetToDoByTitle(query);

            // Assert
            result.Should().BeEquivalentTo(expectedResults);
        }

        [Theory]
        [InlineData("Ga Ada")]
        public void GetToDoByTitle_NotFound(string query)
        {
            // Arrange
            ResetMockData();
            var expectedResults = ToDoItemMockData.ToDoItems.Where(t => t.Title.ToLower().Contains(query.ToLower())).OrderBy(t => t.Title).ToList();

            // Act
            var result = _toDoService.GetToDoByTitle(query);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
