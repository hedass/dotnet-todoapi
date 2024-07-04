using onboarding.dal.Interface;
using onboarding.dal.Interfaces;

namespace onboarding.dal.Repository
{
    public class UnitOfWork : IUnitOfWork
    {

        private readonly ToDoItemDbContext _db;

        public IToDoRepository ToDoRepository { get; }

        public UnitOfWork(ToDoItemDbContext db)
        {
            _db = db;
            ToDoRepository = new ToDoRepository(db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }


    }
}
