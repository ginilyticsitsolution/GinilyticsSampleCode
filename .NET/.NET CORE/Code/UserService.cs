using FoundryLaw.Common.CustomExceptions;
using FoundryLaw.Common.Utility.Contract;
using FoundryLaw.Model.ViewModels;
using FoundryLaw.Service.Data.Model.Contract;
using FoundryLaw.Service.Services.Contract;
using FoundryLaw.Service.Setup;
using Mapster;

namespace FoundryLaw.Service.Services
{
    [ScopedService]
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordSecurity _passwordSecurity;
        private readonly IServiceResult _serviceResultSuccessResponse;
        private readonly IServiceResult _serviceResultErrorResponse;
        private readonly GlobalLogic _globalLogic;

        public UserService(IUserRepository userRepository, IPasswordSecurity passwordSecurity,
            IServiceResult serviceSuccessResult, IServiceResult serviceErrorResult)

        {
            _userRepository = userRepository;
            _passwordSecurity = passwordSecurity;
            _globalLogic = new GlobalLogic();
            _serviceResultSuccessResponse = serviceSuccessResult;
            _serviceResultErrorResponse = serviceErrorResult;
            _serviceResultSuccessResponse = _globalLogic.BuildServiceResult(serviceSuccessResult, true);
            _serviceResultErrorResponse = _globalLogic.BuildServiceResult(serviceErrorResult, false);
          
        }

        public async Task<IServiceResult> DeleteUser(int id, int modifiedBy)
        {
            var destObject = new UserDeleteDto()
            {
                id = id,
                modifiedBy = modifiedBy,
                utcDateDeleted = DateTime.UtcNow
            };
            var result = await _userRepository.DeleteUser(destObject);
            if (result)
            {
                _serviceResultSuccessResponse.ResultData = result;
                return _serviceResultSuccessResponse;
            }
            else
            {
                return _serviceResultErrorResponse;
            }
        }

        public async Task<IServiceResult> GetUserList()
        {
            var result = await _userRepository.GetUserList();
            _serviceResultSuccessResponse.ResultData = result;
            return _serviceResultSuccessResponse;
        }

        public async Task<IServiceResult> GetUserById(int id)
        {
            var result = await _userRepository.GetUserById(id);
            _serviceResultSuccessResponse.ResultData = result;
            return _serviceResultSuccessResponse;
        }

        public async Task<IServiceResult> GetUserByUserName(string userName)
        {
            var result = await _userRepository.GetUserByUserName(userName);
            _serviceResultSuccessResponse.ResultData = result;
            return _serviceResultSuccessResponse;
        }

        public async Task<IServiceResult> InsertUser(UserCreateViewModel user, int createdBy)
        {
            var userName = await _userRepository.GetUserByUserName(user.userName);
            if (userName == null)
            {
                var destObject = user.Adapt(new UserCreateDto());
                destObject.utcDateCreated = DateTime.UtcNow;
                destObject.createdBy = createdBy;
                destObject.password = _passwordSecurity.CreateHash(user.password);
                destObject.isActive = true;
                var id = await _userRepository.InsertUser(destObject);
                if (id > 0)
                {
                    _serviceResultSuccessResponse.ResultData = new { userId = id };
                    return _serviceResultSuccessResponse;
                }
                else
                {
                    return _serviceResultErrorResponse;
                }
            }
            else
            {
                throw new InternalValidationException("User with the given userName already exists");
            }
        }

        public async Task<IServiceResult> UpdateUser(UserUpdateViewModel user, int modifiedBy)
        {
            var destObject = user.Adapt(new UserUpdateDto());
            destObject.utcDateModified = DateTime.UtcNow;
            destObject.modifiedBy = modifiedBy;
            var result = await _userRepository.UpdateUser(destObject);
            if (result)
            {
                _serviceResultSuccessResponse.ResultData = result;
                return _serviceResultSuccessResponse;
            }
            else
            {
                return _serviceResultErrorResponse;
            }
        }
    }
}

