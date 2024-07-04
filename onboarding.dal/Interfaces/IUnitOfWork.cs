using onboarding.dal.Interfaces;
using onboarding.dal.Repository;

namespace onboarding.dal.Interface
{
    public interface IUnitOfWork
    {
        public IToDoRepository ToDoRepository { get; }
        public void Save();
    }
}
