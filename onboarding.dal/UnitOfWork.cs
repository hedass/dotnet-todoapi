namespace onboarding.dal
{
    public class UnitOfWork
    {

        private readonly ToDoItemDbContext _db;

        public ToDoRepository ToDoRepository { get; }

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
