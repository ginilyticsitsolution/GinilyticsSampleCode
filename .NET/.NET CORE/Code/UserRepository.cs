using FoundryLaw.Model.ViewModels;
using System.Data;

namespace FoundryLaw.Repository.Repositories
{
    [SingletonService]
    public class UserRepository : IUserRepository
    {
        private readonly IDbProvider _dbProvider;

        public UserRepository(IDbProvider dbProvider)
        {
            _dbProvider = dbProvider;
        }

        public async Task<bool> DeleteUser(UserDeleteDto user)
        {
            await _dbProvider.ExecuteAsync("[user].[sp_deleteUser]", user, CommandType.StoredProcedure);
            return true;
        }

        public async Task<List<UserDetailViewModel>> GetUserList()
        {
            var result = await _dbProvider.ExecuteQueryAsync<UserDetailViewModel>("[user].[sp_getUserList]");
            return result.ToList();
        }

        public async Task<UserDetailViewModel> GetUserById(int id)
        {
            var param = new { id };
            var result = await _dbProvider.ExecuteQueryAsync<UserDetailViewModel>("[user].[sp_getUserById]", param);
            return result.FirstOrDefault();
        }

        public async Task<UserDetailViewModel> GetUserByUserName(string userName)
        {
            var param = new { userName };
            var result = await _dbProvider.ExecuteQueryAsync<UserDetailViewModel>("[user].[sp_getUserByUserName]", param);
            return result.FirstOrDefault();
        }

        public async Task<int> InsertUser(UserCreateDto user)
        {
            var createdId = await _dbProvider.ExecuteScalarAsync<int>("[user].[sp_insertUser]", user);
            return createdId;
        }

        public async Task<bool> UpdateUser(UserUpdateDto user)
        {
            await _dbProvider.ExecuteAsync("[user].[sp_updateUser]", user);
            return true;
        }
    }
}
